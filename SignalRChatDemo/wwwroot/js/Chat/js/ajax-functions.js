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
    $mensagens.scrollTop($mensagens[0].scrollHeight);
}

function selecionarContato(to, myNumberId) {
    $.ajax({
        url: '/Chat/ContatoSelecionado',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ WebId: to, MyNumberId: myNumberId }),
        success: function (html) {
            globalIdContact = to;
           
            $('.chat-container').html(html);

            AtivarScrollChatArea();
        }
    });
}

function AtualizarListaDeContatos() {
    $.ajax({
        url: '/Chat/AtualizaListaDeContatos',
        type: 'GET',
        success: function (html) {
            $('.conversas').html(html);
        }
    });
}

function SendStatusMessageToController(from, to) {
    $.ajax({
        url: '/Chat/AtualizaStatusMessage',
        type: 'GET',
        data: { WIDFrom: from, WIDTo: to },
        success: function (html) {
            // nao faz nada, ainda.
        }
    });
}

function SendMessageToController(from, message, datetime) {
    $.ajax({
        url: '/Chat/SalvarMensagem',
        type: 'POST',
        data: { WIDFrom: from, WIDTo: globalIdContact, message: message, datetime: datetime },
        success: function (html) {
            console.log(html);
        }
    });
}