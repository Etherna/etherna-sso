// Minimal WebAuthn client helper for FIDO2 second factor.
// Exposes window.ethernaFido2 with createCredential() and assertCredential().
// Bridges Fido2NetLib's base64url-encoded JSON options to the browser's
// PublicKeyCredential API and serializes the response back to JSON.

declare global {
    interface Window {
        ethernaFido2: {
            createCredential: (options: any) => Promise<string>
            assertCredential: (options: any) => Promise<string>
        }
    }
}

function base64UrlToArrayBuffer(value: string): ArrayBuffer {
    const padded = value.replace(/-/g, "+").replace(/_/g, "/") + "===".slice((value.length + 3) % 4)
    const binary = atob(padded)
    const bytes = new Uint8Array(binary.length)
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i)
    return bytes.buffer
}

function arrayBufferToBase64Url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer)
    let binary = ""
    for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i])
    return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "")
}

function decodeCredentials(list: any[]): any[] {
    return (list ?? []).map(c => ({ ...c, id: base64UrlToArrayBuffer(c.id) }))
}

async function createCredential(options: any): Promise<string> {
    const publicKey: PublicKeyCredentialCreationOptions = {
        ...options,
        challenge: base64UrlToArrayBuffer(options.challenge),
        user: { ...options.user, id: base64UrlToArrayBuffer(options.user.id) },
        excludeCredentials: decodeCredentials(options.excludeCredentials)
    }

    const credential = await navigator.credentials.create({ publicKey }) as PublicKeyCredential | null
    if (!credential) throw new Error("Registration cancelled.")

    const response = credential.response as AuthenticatorAttestationResponse
    const transports = typeof response.getTransports === "function" ? response.getTransports() : []

    return JSON.stringify({
        id: credential.id,
        rawId: arrayBufferToBase64Url(credential.rawId),
        type: credential.type,
        response: {
            attestationObject: arrayBufferToBase64Url(response.attestationObject),
            clientDataJSON: arrayBufferToBase64Url(response.clientDataJSON),
            transports
        },
        extensions: credential.getClientExtensionResults()
    })
}

async function assertCredential(options: any): Promise<string> {
    const publicKey: PublicKeyCredentialRequestOptions = {
        ...options,
        challenge: base64UrlToArrayBuffer(options.challenge),
        allowCredentials: decodeCredentials(options.allowCredentials)
    }

    const credential = await navigator.credentials.get({ publicKey }) as PublicKeyCredential | null
    if (!credential) throw new Error("Authentication cancelled.")

    const response = credential.response as AuthenticatorAssertionResponse

    return JSON.stringify({
        id: credential.id,
        rawId: arrayBufferToBase64Url(credential.rawId),
        type: credential.type,
        response: {
            authenticatorData: arrayBufferToBase64Url(response.authenticatorData),
            clientDataJSON: arrayBufferToBase64Url(response.clientDataJSON),
            signature: arrayBufferToBase64Url(response.signature),
            userHandle: response.userHandle ? arrayBufferToBase64Url(response.userHandle) : null
        },
        extensions: credential.getClientExtensionResults()
    })
}

window.ethernaFido2 = { createCredential, assertCredential }
export {}
