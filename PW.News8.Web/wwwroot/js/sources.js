

function notifyError(message) {
    if (window.Swal) {
        Swal.fire({ icon: 'error', title: 'Ocurrió un error', text: message });
    } else {
        alert(message);
    }
}

function notifySuccess(message) {
    if (window.Swal) {
        Swal.fire({ icon: 'success', title: '¡Listo!', text: message, timer: 2500, showConfirmButton: false });
    } else {
        alert(message);
    }
}

// Listado de fuentes 
function loadSourcesList() {
    const $container = $('#sources-container');
    $container.html('<div class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Cargando fuentes…</div>');

    $.ajax({
        url: '/Sources/ListPartial',
        type: 'GET'
    }).done(function (html) {
        $container.html(html);
    }).fail(function () {
        $container.html('<div class="alert alert-danger">No se pudo cargar el listado de fuentes. Verifique que la API esté en ejecución.</div>');
    });
}

//  Listado de ítems guardados (Sources/Items) 
function loadItemsList(sourceId) {
    const $container = $('#items-container');
    $container.html('<div class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Cargando ítems…</div>');

    $.ajax({
        url: '/Sources/ItemsPartial/' + sourceId,
        type: 'GET'
    }).done(function (html) {
        $container.html(html);
        bindJsonModalButtons();
    }).fail(function () {
        $container.html('<div class="alert alert-danger">No se pudieron cargar los ítems. Verifique que la API esté en ejecución.</div>');
    });
}

//Lectura en vivo (fetch) 
function fetchLive(sourceId) {
    const $panel = $('#live-fetch-panel');
    const $body = $('#live-fetch-body');

    $panel.removeClass('d-none');
    $body.html('<div class="text-center text-muted py-4"><div class="spinner-border spinner-border-sm me-2"></div>Leyendo la fuente en vivo…</div>');

    $.ajax({
        url: '/Sources/FetchLive/' + sourceId,
        type: 'GET'
    }).done(function (result) {
        renderLiveFetchResult(sourceId, result, $body);
    }).fail(function (xhr) {
        const result = xhr.responseJSON;
        const message = (result && result.errorMessage) || 'No se pudo leer la fuente en vivo.';
        $body.html('<div class="alert alert-warning mb-0">' + message + '</div>');
    });
}

function renderLiveFetchResult(sourceId, result, $body) {
    if (!result || !result.success) {
        $body.html('<div class="alert alert-warning mb-0">' + (result && result.errorMessage ? result.errorMessage : 'La fuente no devolvió resultados.') + '</div>');
        return;
    }

    const items = result.items || [];
    if (items.length === 0) {
        $body.html('<p class="text-muted mb-0">La fuente no devolvió ítems en este momento.</p>');
        return;
    }

    let html = '<div class="list-group">';
    items.forEach(function (item, index) {
        const title = item.title || item.Title || item.name || item.Name || ('Ítem #' + (index + 1));
        const preview = JSON.stringify(item);
        const shortPreview = preview.length > 140 ? preview.substring(0, 140) + '…' : preview;

        html += '<div class="list-group-item d-flex justify-content-between align-items-center gap-3">';
        html += '  <div class="text-truncate">';
        html += '    <div class="fw-semibold text-truncate">' + $('<div>').text(title).html() + '</div>';
        html += '    <div class="small text-muted text-truncate">' + $('<div>').text(shortPreview).html() + '</div>';
        html += '  </div>';
        html += '  <button type="button" class="btn btn-sm btn-success btn-save-item flex-shrink-0" data-index="' + index + '">Guardar</button>';
        html += '</div>';
    });
    html += '</div>';

    $body.html(html);

    $body.find('.btn-save-item').on('click', function () {
        const idx = $(this).data('index');
        saveFetchedItem(sourceId, items[idx], $(this));
    });
}

function saveFetchedItem(sourceId, item, $button) {
    $button.prop('disabled', true).text('Guardando…');

    $.ajax({
        url: '/Sources/SaveItem/' + sourceId,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(item)
    }).done(function () {
        notifySuccess('El ítem se guardó correctamente.');
        $button.closest('.list-group-item').fadeOut(200, function () { $(this).remove(); });
        loadItemsList(sourceId);
    }).fail(function (xhr) {
        const message = (xhr.responseJSON && xhr.responseJSON.message) || 'No se pudo guardar el ítem.';
        notifyError(message);
        $button.prop('disabled', false).text('Guardar');
    });
}

//  Modal de detalle JSON
function bindJsonModalButtons() {
    $('.btn-view-json').off('click').on('click', function () {
        const raw = $(this).attr('data-json');
        let pretty = raw;
        try {
            pretty = JSON.stringify(JSON.parse(raw), null, 2);
        } catch (e) {
            // Si no es JSON válido se muestra tal cual.
        }
        $('#jsonModalBody').text(pretty);
        const modal = new bootstrap.Modal(document.getElementById('jsonModal'));
        modal.show();
    });
}

// Subida de archivo JSON 
function uploadJsonFile(file) {
    if (!file) {
        notifyError('Debe seleccionar un archivo .json.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    const $btn = $('#btn-upload');
    const $spinner = $('#upload-spinner');
    $btn.prop('disabled', true);
    $spinner.removeClass('d-none');

    $.ajax({
        url: '/Sources/Upload',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false
    }).done(function (result) {
        notifySuccess(result.message || 'Archivo procesado correctamente.');
        setTimeout(function () {
            window.location.href = '/Sources' + (result.sourceId ? '/Items/' + result.sourceId : '');
        }, 1200);
    }).fail(function (xhr) {
        const message = (xhr.responseJSON && xhr.responseJSON.message) || 'No se pudo procesar el archivo.';
        notifyError(message);
    }).always(function () {
        $btn.prop('disabled', false);
        $spinner.addClass('d-none');
    });
}