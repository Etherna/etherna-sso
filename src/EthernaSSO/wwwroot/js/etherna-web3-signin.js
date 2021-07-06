
var EthernaWeb3Signin = {
    constants: {
        confirmWeb3Selector: '#confirm-web3-login',
        loginWeb3Selector: '#web3-login-btn',
        errorAlertSelector: '#manage-web3-login-alert',

        web3LoginViewSelector: '#web3-login',
        manageWeb3ViewSelector: '#manage-web3-login',
        installMetamaskViewSelector: '#install-metamask',

        // Values below must be updated in the specific page
        retriveAuthMessageUrl: null,
        confirmSignatureUrl: null
    },

    load: function () {
        if (typeof window.web3 !== "undefined") {
            $(EthernaWeb3Signin.constants.web3LoginViewSelector).show();
            $(EthernaWeb3Signin.constants.manageWeb3ViewSelector).show();
        } else {
            $(EthernaWeb3Signin.constants.installMetamaskViewSelector).show();
        }

        $(EthernaWeb3Signin.constants.loginWeb3Selector).click(EthernaWeb3Signin.web3Signin)
        $(EthernaWeb3Signin.constants.confirmWeb3Selector).click(EthernaWeb3Signin.web3Signin)
    },

    web3Signin: function () {
        EthernaWeb3Signin.hideError();

        if (typeof window.ethereum !== 'undefined') {
            window.web3 = new Web3(window.ethereum);
            window.ethereum.enable().then(EthernaWeb3Signin.getSignMsg).catch(EthernaWeb3Signin.showError);
        } else {
            window.web3.eth.getAccounts().then(EthernaWeb3Signin.getSignMsg).catch(EthernaWeb3Signin.showError);
        }
    },

    getSignMsg: function (accounts) {
        if (accounts && accounts.length) {
            var web3 = window.web3;
            var address = (web3.utils || web3).toChecksumAddress(accounts[0]);
            var msgUrl = EthernaWeb3Signin.constants.retriveAuthMessageUrl + '&etherAddress=' + address;
            $.ajax({
                url: msgUrl,
                success: function (msg) {
                    EthernaWeb3Signin.signAndRedirect(msg, address)
                },
                beforeSend: function () {
                    $(EthernaWeb3Signin.constants.confirmWeb3Selector).prop('disabled', true);
                },
                complete: function () {
                    $(EthernaWeb3Signin.constants.confirmWeb3Selector).prop('disabled', false);
                },
                error: EthernaWeb3Signin.showError
            });
        } else {
            EthernaWeb3Signin.showError();
        }
    },

    signAndRedirect: function (msg, address) {
        var confirmSignatureUrl = EthernaWeb3Signin.constants.confirmSignatureUrl;
        function signCallback(newSig, oldSig) {
            const sig = oldSig || newSig;
            if (typeof sig === "string") {
                var redirect = confirmSignatureUrl + '&etherAddress=' + address + '&signature=' + sig;
                window.location.href = redirect;
            } else {
                EthernaWeb3Signin.showError();
            }
        }

        var web3 = window.web3;
        if (web3.eth.personal) {
            // new web3
            web3.eth.personal.sign(msg, address).then(signCallback).catch(EthernaWeb3Signin.showError);
        } else {
            // old web3
            web3.personal.sign(msg, address, signCallback);
        }
    },

    showError: function (error) {
        var msg = error && error.message || 'Cannot get the account address. Make sure your wallet is up to date.';
        $(EthernaWeb3Signin.constants.errorAlertSelector).show();
        $(EthernaWeb3Signin.constants.errorAlertSelector).text(msg);
    },

    hideError: function () {
        $(EthernaWeb3Signin.constants.errorAlertSelector).hide();
    }
}

window.addEventListener('load', EthernaWeb3Signin.load)
