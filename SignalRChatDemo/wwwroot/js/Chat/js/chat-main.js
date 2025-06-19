let currentContact = "";
let typingTimeout = { timeoutId: null };

$(function () {
    IniciarSignalR(OwnerNumberId, listContacts, typingTimeout, SendMessageToController)
});