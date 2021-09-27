import { providers } from "ethers"

export { }

declare global {
  interface Window {
    web3?: {
      currentProvider: providers.ExternalProvider
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
    retriveAuthMessageUrl?: string | null
    confirmSignatureUrl?: string | null
  }
}
