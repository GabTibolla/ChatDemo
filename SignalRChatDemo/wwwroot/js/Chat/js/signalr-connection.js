
let connection;

function IniciarSignalR(from, contacts, typingTimeout, sendMessageFunc) {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub?userName=" + encodeURIComponent(from))
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
        SetupEvents(connection, from, contacts, typingTimeout, sendMessageFunc);
    })
    .catch(function (err) {
      return console.error(err.toString());
    });
}
