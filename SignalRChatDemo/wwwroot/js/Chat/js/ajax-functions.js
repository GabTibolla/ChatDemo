function OpenModal(opened)
{
    if (opened) {
        $("#addUserModal").css("display", "flex");
    } else {
        $("#addUserModal").css("display", "none");
    }
}

$(document).ready(function () {
    $("#addButton").click(function () {
        $("#addUserModal").css("display", "flex");
    });

    $('#AddNumberId').on('input', function () {
        let val = $(this).val().toUpperCase().replace(/[^A-Z0-9]/g, '');

        // Aplica o formato XX-0000-X0
        let formatado = '';
        if (val.length > 0) formatado += val[0];
        if (val.length > 1) formatado += val[1];
        if (val.length > 2) formatado += '-' + val[2];
        if (val.length > 3) formatado += val[3];
        if (val.length > 4) formatado += val[4];
        if (val.length > 5) formatado += val[5];
        if (val.length > 6) formatado += '-' + val[6];
        if (val.length > 7) formatado += val[7];

        $(this).val(formatado);
    });

    $(".modal-cancel").click(function () {
        $("#addUserModal").css("display", "none");
    });
});

$(document).ready(function () {
    $("#btnMenuCard").click(function (e) {
        e.stopPropagation();

        if ($(".menu-dropdown").css("display") === "flex") {
            $(".menu-dropdown").css("display", "none");
        } else {
            $(".menu-dropdown").css("display", "flex");
        }
    });

    $("#btnLogout").click(function () {
        $.ajax({
            url: "/Login/LogOut",
            success: function (response) {
                window.location.href = response.redirectUrl;
            },
        });
    });
});

$(document).on("click", function (e) {
    // Se o clique NÃO foi no botão e nem no menu
    if (!$(e.target).closest(".menu-dropdown").length && !$(e.target).is("#btnMenuCard")) {
        $(".menu-dropdown").css("display", "none");
    }
});

$(document).on('keypress', '#chatInput', function (e) {
    if (e.which === 13 && !e.shiftKey) {
        e.preventDefault();
        $('#btnSendMessage').click();
    }
});

function AtivarScrollChatArea() {
    let $mensagens = $('#chatMessages');

    if ($mensagens.length > 0) {
        $mensagens.scrollTop($mensagens[0].scrollHeight);
    }
}

function SelectConversation(ContactNumberId, OwnerNumberId) {
    $.ajax({
        url: '/Chat/ConversationSelected',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ ContactNumberId: ContactNumberId, OwnerNumberId: OwnerNumberId }),
        success: function (html) {
            currentContact = ContactNumberId;
            $('.chat-container').html(html);        
            AtivarScrollChatArea();

            MarkMessagesToRead(ContactNumberId, OwnerNumberId);
        }
    });
}

function MarkMessagesToRead(ContactNumberId, OwnerNumberId) {
    $.ajax({
        url: '/Chat/ReturnMessagesUnread',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ ContactNumberId: ContactNumberId, OwnerNumberId: OwnerNumberId }),
        success: function (resposta) {
            console.log(resposta);

            if (resposta.messages.length > 0) {
                resposta.messages.forEach(item => {
                    SendStatusMessage(connection, OwnerNumberId, ContactNumberId,  item.webId);
                });
            }
        }
    });
}

function AtualizarListaDeContatos() {
    $.ajax({
        url: '/Chat/UpdateContactsList',
        type: 'GET',
        success: function (html) {
            $('.conversas').html(html);
        }
    });
}

function SendMessageToController(OwnerNumberId, message, guidMessage, datetime) {
    $.ajax({
        url: '/Chat/SaveMessage',
        type: 'POST',
        data: { OwnerNumberId: OwnerNumberId, ContactNumberId: currentContact, Message: message, GuidMessage: guidMessage, datetime: datetime },
        success: function (html) {
            console.log(html);
        }
    });
}

function SendStatusMessageToController(OwnerNumberId, ContactNumberId) {
    $.ajax({
        url: '/Chat/UpdateStatusMessage',
        type: 'GET',
        data: { OwnerNumberId: OwnerNumberId, ContactNumberId: ContactNumberId },
        success: function (html) {
            // nao faz nada, ainda.
        }
    });
}