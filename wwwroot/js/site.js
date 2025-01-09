// Alerta y Redireccion
async function alertAndRedirect(message, redirectUrl) {
    alert(message); // Pausa la ejecución hasta que el usuario presione "Aceptar"
    window.location.href = redirectUrl; // Redirige después de cerrar la alerta
}

//***************************************** CONFIGURO SIGNALR *****************************************///////////////


// Crear la conexión de SignalR si no existe
if (!window.signalRConnection) {
    window.signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl("/UpdateHub")
        .build();
    // Inicia la conexión y maneja errores
    window.signalRConnection.start()
        .then(() => {
            console.log("Conexión exitosa a SignalR");
        })
        .catch(function (err) {
            console.log("Error de conexión:", err.toString());
        });
}

// Escuchar la notificación "ReceiveTableUpdate" del servidor
window.signalRConnection.on("ReceiveTableUpdate", () => {
    
    updateTableData();
});

// Escuchar la notificación "ReceiveNotify" del servidor
window.signalRConnection.on("ReceiveNotify", (title, message) => {

    notificacion(title, message);
});

function updateTableData() {
    // Lógica para actualizar la tabla en la vista del cliente
    console.log("Actualizando la tabla...");
    const table = $('#tabla1').DataTable();
   
    table.ajax.reload(null, false);
  
};
function initializeTooltips() {
    // Destruir tooltips existentes
    const existingTooltips = document.querySelectorAll('.tooltip');
    existingTooltips.forEach(tooltip => tooltip.remove());

    // Inicializar nuevos tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(tooltipTriggerEl => {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
}



// Inicializar tooltips en la tabla después de que se dibuje
$('#tabla1').on('draw.dt', function () {
    initializeTooltips();
});

// Inicializar tooltips en el sidebar
document.addEventListener('DOMContentLoaded', function () {
    initializeTooltips();
});


window.signalRConnection.onclose(async () => {
    console.log("Conexión perdida, intentando reconectar...");
    await window.signalRConnection.start()
        .then(() => console.log("Reconexión exitosa"))
        .catch(err => console.error("Error al reconectar:", err.toString()));
});

///***************************************** Manejo de los Paneles moviles *****************************///
$(function () {
    // Resizable del panel derecho
    $(".wright.pane").resizable({
        handles: "w",
        minWidth: 200,   
        maxWidth: 1250,
        resize: function (event, ui) {
            ui.position.left = 0; // Mantener el panel derecho alineado a la derecha           
        },      
    });
});

// ******************* notificaciones *********************************//
function notificacion(encabezado, cuerpo) {
    // Verificar compatibilidad con la API de notificaciones
    if (!("Notification" in window)) {
        console.error("Este navegador no soporta notificaciones.");
        return;
    }

    // Verificar estado del permiso
    if (Notification.permission === "granted") {
        // Mostrar la notificación directamente
        new Notification(encabezado, { body: cuerpo, icon: "Images/campana.png" });
    } else if (Notification.permission !== "denied") {
        // Solicitar permiso al usuario
        Notification.requestPermission()
            .then(permission => {
                if (permission === "granted") {
                    new Notification(encabezado, { body: cuerpo, icon: "Images/campana.png" });
                } else {
                    console.warn("El usuario ha denegado las notificaciones.");
                }
            })
            .catch(error => {
                console.error("Error al solicitar permiso para notificaciones:", error);
            });
    } else {
        console.warn("Las notificaciones han sido denegadas previamente.");
    }
}

//**************** Funciones para la Tabla ********************************************/
function adjustTable() {
    var table = $('#tabla1').DataTable();
    table.columns.adjust();
    table.responsive.rebuild();
    table.responsive.recalc();
}
$(document).ready(function () {
    $('#tabla1 tbody').on('click', 'tr', function () {
        // Si ya hay una fila seleccionada, eliminamos la clase 'selected'
        $('#tabla1 tbody tr').removeClass('seleccionada');

        // Añadimos la clase 'selected' a la fila clickeada
        $(this).addClass('seleccionada');
    });
});

//********************** Alertas Personalizadas *********************/

// Custom alert
function ShowAlert(title, message) {
    $('#alertTitle').text(title);
    $('#alertMessage').text(message);
    $('#customAlert').fadeIn();

    $('#alertOkButton').off('click').on('click', function () {
        $('#customAlert').fadeOut();
    });
}

// Custom confirm
async function ShowConfirm(title, message) {
    $('#confirmTitle').text(title);
    $('#confirmMessage').text(message);
    $('#customConfirm').fadeIn();

    return new Promise((resolve) => {
        $('#confirmYesButton').off('click').on('click', function () {
            $('#customConfirm').fadeOut();
            resolve(true); // Resolución de la promesa con `true`
        });

        $('#confirmNoButton').off('click').on('click', function () {
            $('#customConfirm').fadeOut();
            resolve(false); // Resolución de la promesa con `false`
        });
    });
}

function CustomAlertAndRedirect(title, message, redirectUrl) {
    $('#alertTitle').text(title);
    $('#alertMessage').text(message);
    $('#customAlert').fadeIn();

    $('#alertOkButton').off('click').on('click', function () {
        $('#customAlert').fadeOut();

        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    });
}

//************************************************************************************************************//