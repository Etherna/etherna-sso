
var EthernaWeb3Singin = {
    constants: {
        changeWeb3Selector: '#change-web3-login',
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
            $(EthernaWeb3Singin.constants.web3LoginViewSelector).show();
            $(EthernaWeb3Singin.constants.manageWeb3ViewSelector).show();
        } else {
            $(EthernaWeb3Singin.constants.installMetamaskViewSelector).show();
        }

        $(EthernaWeb3Singin.constants.loginWeb3Selector).click(EthernaWeb3Singin.web3Signin)
        $(EthernaWeb3Singin.constants.changeWeb3Selector).click(EthernaWeb3Singin.web3Signin)
    },

    web3Signin: function () {
        EthernaWeb3Singin.hideError();

        if (typeof window.ethereum !== 'undefined') {
            window.web3 = new Web3(window.ethereum);
            window.ethereum.enable().then(EthernaWeb3Singin.getSignMsg).catch(EthernaWeb3Singin.showError);
        } else {
            window.web3.eth.getAccounts().then(EthernaWeb3Singin.getSignMsg).catch(EthernaWeb3Singin.showError);
        }
    },

    getSignMsg: function (accounts) {
        if (accounts && accounts.length) {
            var web3 = window.web3;
            var address = (web3.utils || web3).toChecksumAddress(accounts[0]);
            var msgUrl = EthernaWeb3Singin.constants.retriveAuthMessageUrl + '&etherAddress=' + address;
            $.ajax({
                url: msgUrl,
                success: function (msg) {
                    EthernaWeb3Singin.signAndRedirect(msg, address)
                },
                beforeSend: function () {
                    $(EthernaWeb3Singin.constants.changeWeb3Selector).prop('disabled', true);
                },
                complete: function () {
                    $(EthernaWeb3Singin.constants.changeWeb3Selector).prop('disabled', false);
                },
                error: EthernaWeb3Singin.showError
            });
        } else {
            EthernaWeb3Singin.showError();
        }
    },

    signAndRedirect: function (msg, address) {
        var confirmSignatureUrl = EthernaWeb3Singin.constants.confirmSignatureUrl;
        function signCallback(newSig, oldSig) {
            const sig = oldSig || newSig;
            if (typeof sig === "string") {
                var redirect = confirmSignatureUrl + '&etherAddress=' + address + '&signature=' + sig;
                window.location.href = redirect;
            } else {
                EthernaWeb3Singin.showError();
            }
        }

        var web3 = window.web3;
        if (web3.eth.personal) {
            // new web3
            web3.eth.personal.sign(msg, address).then(signCallback).catch(EthernaWeb3Singin.showError);
        } else {
            // old web3
            web3.personal.sign(msg, address, signCallback);
        }
    },

    showError: function (error) {
        var msg = error && error.message || 'Cannot get the account address. Make sure your wallet is up to date.';
        $(EthernaWeb3Singin.constants.errorAlertSelector).show();
        $(EthernaWeb3Singin.constants.errorAlertSelector).text(msg);
    },

    hideError: function () {
        $(EthernaWeb3Singin.constants.errorAlertSelector).hide();
    }
}

window.addEventListener('load', EthernaWeb3Singin.load)
