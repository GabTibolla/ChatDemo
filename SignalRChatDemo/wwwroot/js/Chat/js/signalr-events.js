
function SetupEvents(connection, from, contacts, typingTimeout, sendMessageFunc)
{
    // Emits
    SendTyping(connection, from);
    SendMessage(connection, from);

    // Receives
    ReceiveMessage(connection, contacts);
    ReceiveTyping(connection, typingTimeout);
}

function SendTyping(connection, from)
{
    $(document).on('input', '#chatInput', function () {
        connection.invoke("Typing", from, globalIdContact).catch(function (err) {
            return console.error(err.toString());
        });
    });
}

function SendMessage(connection, from)
{
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

        connection.invoke("SendMessage", from, globalIdContact, message, time).catch(function (err) {
            return console.error(err.toString());
        });

        const chatMessages = $("#chatMessages");
        chatMessages.append("<div class='message sent'>" + "<div class='message-hour'>" + time + "</div>" + message + "</div>");
        chatMessages.scrollTop(chatMessages[0].scrollHeight);
        $("#chatInput").val("");

        SendMessageToController(from, message, datetime);
        AtualizarListaDeContatos();
    });
}

function ReceiveMessage(connection, contacts) {
    connection.on("ReceiveMessage", function (to, message, time) {

        if (to == globalIdContact) {
            const chatMessages = $("#chatMessages");
            chatMessages.append("<div class='message received'>" + "<div class='message-hour'>" + time + "</div>" + message + "</div>");
            chatMessages.scrollTop(chatMessages[0].scrollHeight);
        }

        AtualizarListaDeContatos();
    });
}

function ReceiveTyping(connection, typingTimeout) {
    connection.on("UserTyping", function (to) {
        $("#typing-indicator-" + to).text("digitando...");

        $("#last-message-" + to).css("display", "none");

        if (to == globalIdContact) {
            $("#typing-chatArea-" + to).text("digitando...");
        }
        clearTimeout(typingTimeout);

        typingTimeout = setTimeout(function () {
            $("#typing-indicator-" + to).text("");
            $("#typing-chatArea-" + to).text("");

            $("#last-message-" + to).css("display", "flex");
        }, 700);
    });
}