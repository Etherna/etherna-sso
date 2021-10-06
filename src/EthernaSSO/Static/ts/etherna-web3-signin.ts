import { providers, utils } from "ethers"

interface UIStore {
  // web3
  web3Provider?: providers.Web3Provider
  // components
  web3LoginButton?: HTMLButtonElement
  confirmWeb3LoginButton?: HTMLButtonElement
  errorAlert?: HTMLDivElement
  web3LoginView?: HTMLDivElement
  manageWeb3View?: HTMLDivElement
  installMetamaskView?: HTMLDivElement
  // vars
  retriveAuthMessageUrl: string | null
  confirmSignatureUrl: string | null
}

const store: UIStore = {
  retriveAuthMessageUrl: window.retriveAuthMessageUrl,
  confirmSignatureUrl: window.confirmSignatureUrl
}

window.addEventListener("load", load)

function load() {
  store.web3LoginButton = document.querySelector("#web3-login-btn")
  store.confirmWeb3LoginButton = document.querySelector("#confirm-web3-login")
  store.errorAlert = document.querySelector("#manage-web3-login-alert")
  store.web3LoginView = document.querySelector("#web3-login")
  store.manageWeb3View = document.querySelector("#manage-web3-login")
  store.installMetamaskView = document.querySelector("#install-metamask")

  if (typeof window.web3 !== "undefined") {
    showEl(store.web3LoginView)
    showEl(store.manageWeb3View)
  } else {
    showEl(store.installMetamaskView)
  }

  store.web3LoginButton?.addEventListener("click", web3Signin)
  store.confirmWeb3LoginButton?.addEventListener("click", web3Signin)
}

function showEl(el: HTMLElement | null) {
  if (el) {
    el.style.display = ""
  }
}

function hideEl(el: HTMLElement | null) {
  if (el) {
    el.style.display = "none"
  }
}

function setBtnDisabled(btn: HTMLButtonElement | null, disabled: boolean) {
  if (!btn) return

  btn.disabled = disabled
}

function showError(error: Error) {
  if (!store.errorAlert) return

  var msg = error && error.message || 'Cannot get the account address. Make sure your wallet is up to date.';
  store.errorAlert.innerText = msg

  showEl(store.errorAlert)
}

function hideError() {
  hideEl(store.errorAlert)
}

async function web3Signin() {
  hideError()

  const external = (window.ethereum || window.web3.currentProvider) as providers.ExternalProvider

  if (external) {
    store.web3Provider = new providers.Web3Provider(external, 1)

    try {
      window.ethereum && window.ethereum.enable()
      const accounts = await store.web3Provider.listAccounts()
      const { msg, address } = await getSignMsg(accounts)
      await signinAndRedirect(msg, address)
    } catch (error) {
      console.error(error)

      setBtnDisabled(store.web3LoginButton, false)
      setBtnDisabled(store.confirmWeb3LoginButton, false)
      showError(error)
    }
  }
}

async function getSignMsg(accounts: string[]) {
  if (!accounts || !accounts.length) throw new Error("Unlock you wallet and try again.")

  const address = utils.getAddress(accounts[0])
  const msgUrl = store.retriveAuthMessageUrl + `&etherAddress=${address}`

  setBtnDisabled(store.web3LoginButton, true)
  setBtnDisabled(store.confirmWeb3LoginButton, true)

  try {
    const resp = await fetch(msgUrl)
    const msg = await resp.json()

    return {
      msg,
      address
    }
  } catch (error) {
    setBtnDisabled(store.web3LoginButton, false)
    setBtnDisabled(store.confirmWeb3LoginButton, false)
    throw error
  }
}

async function signinAndRedirect(msg: string, address: string) {
  const signer = store.web3Provider!.getSigner(address)
  const signature = await signer.signMessage(msg)

  const redirect = store.confirmSignatureUrl + '&etherAddress=' + address + '&signature=' + signature
  window.location.href = redirect
}
