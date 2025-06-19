
function SetupEvents(connection, OwnerNumberId, listContacts, typingTimeout, sendMessageFunc) {
    // Emits
    SendTyping(connection, OwnerNumberId);
    SendMessage(connection, OwnerNumberId);

    // Receives
    ReceiveMessage(connection, OwnerNumberId, listContacts);
    ReceiveStatusMessage(connection);
    ReceiveTyping(connection, typingTimeout);
}

function SendTyping(connection, OwnerNumberId) {
    $(document).on('input', '#chatInput', function () {
        connection.invoke("Typing", OwnerNumberId, currentContact).catch(function (err) {
            return console.error(err.toString());
        });
    });
}

function SendMessage(connection, OwnerNumberId) {
    $(document).on('click', '#btnSendMessage', function () {
        const message = $('#chatInput').val().trim();
        if (message === "") {
            return;
        }

        const now = new Date();
        const datetime = now.toISOString();

        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const time = `${hours}:${minutes}`;

        let guidMessage = crypto.randomUUID();
        connection.invoke("SendMessage", OwnerNumberId, currentContact, message, guidMessage, time).catch(function (err) {
            return console.error(err.toString());
        });

        const chatMessages = $("#chatMessages");
        chatMessages.append("<div class='message sent'>" + "<div class='message-hour'>" + time + '<div id="statusMessage-' + guidMessage + '" class="status-message"></div>' + "</div>" + message + "</div>");
        chatMessages.scrollTop(chatMessages[0].scrollHeight);
        $("#chatInput").val("");

        SendMessageToController(OwnerNumberId, message, guidMessage, datetime);
        AtualizarListaDeContatos();
    });
}

function SendStatusMessage(connection, OwnerNumberId, Contact, GuidMessage) {
    connection.invoke("SendStatusMessage", Contact, GuidMessage).catch(function (err) {
        return console.error(err.toString());
    });

    // envia pro controller que leu a mensagem
    SendStatusMessageToController(OwnerNumberId, Contact);
}

function ReceiveMessage(connection, OwnerNumberId, contacts) {
    connection.on("ReceiveMessage", function (Contact, Message, GuidMessage, time) {

        if (Contact == currentContact) {
            const chatMessages = $("#chatMessages");
            chatMessages.append("<div class='message received'>" + "<div class='message-hour'>" + time + "</div>" + Message + "</div>");
            chatMessages.scrollTop(chatMessages[0].scrollHeight);

            // Atualizar a mensagem para "Lida"
            SendStatusMessage(connection, OwnerNumberId, Contact, GuidMessage);
        }

        AtualizarListaDeContatos();
    });
}

function ReceiveStatusMessage(connection)
{
    connection.on("ReceiveStatusMessage", function (GuidMessage) {

        var idItem = $("#statusMessage-" + GuidMessage);
        // Muda cor do css status-message
        idItem.removeClass("read").addClass("read");
    });
}

function ReceiveTyping(connection, typingTimeout) {
    connection.on("UserTyping", function (Contact) {
        $("#typing-indicator-" + Contact).text("digitando...");

        $("#last-message-" + Contact).css("display", "none");

        if (Contact == currentContact) {
            $("#typing-chatArea-" + Contact).text("digitando...");
        }

        clearTimeout(typingTimeout);

        typingTimeout = setTimeout(function () {
            $("#typing-indicator-" + Contact).text("");
            $("#typing-chatArea-" + Contact).text("");

            $("#last-message-" + Contact).css("display", "flex");
        }, 700);
    });
}