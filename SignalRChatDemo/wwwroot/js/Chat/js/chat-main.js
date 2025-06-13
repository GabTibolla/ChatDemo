let globalIdContact = "";
let typingTimeout = { timeoutId: null };

$(function () {
    IniciarSignalR(from, globalIdContact, contacts, typingTimeout, SendMessageToController)
});