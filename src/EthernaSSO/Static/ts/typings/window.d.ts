import { Eip1193Provider } from "ethers"

export { }

declare global {
  interface Window {
    web3?: {
      currentProvider: Eip1193Provider
    }
    ethereum?: {
      autoRefreshOnNetworkChange: boolean
      chainId: number
      enable: () => Promise<string[]>
      networkVersion: string
      isMetamask?: boolean
      isFortmatic?: boolean
      isPortis?: boolean
      isWalletConnect?: boolean
      isSquarelink?: boolean
      isAuthereum?: boolean
    }
    retrieveAuthMessageUrl?: string | null
    confirmSignatureUrl?: string | null
  }
}
