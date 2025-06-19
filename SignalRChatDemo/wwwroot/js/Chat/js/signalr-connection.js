
let connection;

function IniciarSignalR(OwnerNumberId, listContacts, typingTimeout, sendMessageFunc) {
  connection = new signalR.HubConnectionBuilder()
      .withUrl("/chatHub?userName=" + encodeURIComponent(OwnerNumberId))
    .withAutomaticReconnect()
    .build();

  connection.onreconnecting((error) => {
    console.warn("Tentando reconectar...", error);
  });

  connection.onreconnected((connectionId) => {
    console.log("Reconectado com sucesso!", connectionId);
  });

  return connection.start()
    .then(function () {
        SetupEvents(connection, OwnerNumberId, listContacts, typingTimeout, sendMessageFunc);
    })
    .catch(function (err) {
      return console.error(err.toString());
    });
}
