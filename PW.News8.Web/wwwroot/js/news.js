(function () {
    const STORAGE_KEY = 'pwnews8_news_wall_prefs';

    function getPrefs() {
        try {
            return JSON.parse(localStorage.getItem(STORAGE_KEY)) || {};
        } catch (e) {
            return {};
        }
    }

    function savePrefs(prefs) {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
        } catch (e) {
            // local storage no disponible o lleno, se ignora el error
        }
    }

    function loadWall(params) {
        const $container = $('#news-wall-container');
        $container.html('<div class="text-center text-muted py-5"><div class="spinner-border spinner-border-sm me-2"></div>Cargando noticias…</div>');

        $.ajax({
            url: '/News/WallPartial',
            type: 'GET',
            data: params
        }).done(function (html) {
            $container.html(html);
        }).fail(function () {
            $container.html('<div class="alert alert-danger">No se pudo cargar el muro de noticias. Verifique que la API esté en ejecución.</div>');
        });
    }

    function setMode(mode) {
        $('#news-mode-tabs .nav-link').removeClass('active');
        $('#tab-' + mode).addClass('active');
        $('#panel-headlines').toggleClass('d-none', mode !== 'headlines');
        $('#panel-search').toggleClass('d-none', mode !== 'search');
    }

    function selectCategoryPill(category) {
        $('.category-pill').removeClass('active btn-secondary').addClass('btn-outline-secondary');
        if (category) {
            $('.category-pill[data-category="' + category + '"]')
                .removeClass('btn-outline-secondary').addClass('active btn-secondary');
        }
    }

    function applyHeadlines() {
        const country = $('#filter-country').val();
        const category = $('.category-pill.active').data('category') || '';

        savePrefs({ mode: 'headlines', country: country, category: category });
        loadWall({ mode: 'headlines', country: country, category: category });
    }

    function applySearch() {
        const keyword = ($('#filter-keyword').val() || '').trim();
        const language = $('#filter-language').val();

        if (!keyword) {
            if (window.Swal) {
                Swal.fire({ icon: 'info', title: 'Falta el tema', text: 'Escriba un tema o palabra clave para buscar.' });
            } else {
                alert('Escriba un tema o palabra clave para buscar.');
            }
            return;
        }

        savePrefs({ mode: 'search', keyword: keyword, language: language });
        loadWall({ mode: 'search', q: keyword, language: language });
    }

    $(function () {
        $('#tab-headlines').on('click', function () { setMode('headlines'); });
        $('#tab-search').on('click', function () { setMode('search'); });

        $('.category-pill').on('click', function () {
            const category = $(this).data('category');
            const wasActive = $(this).hasClass('active');
            selectCategoryPill(wasActive ? null : category);
        });

        $('#btn-apply-headlines').on('click', applyHeadlines);
        $('#btn-apply-search').on('click', applySearch);
        $('#filter-keyword').on('keypress', function (e) {
            if (e.which === 13) applySearch();
        });

        // Muro personalizado: se restaura la última preferencia guardada en este navegador.
        // ayuda a que el usuario no tenga que volver a seleccionar país/categoría o tema cada vez que recarga la página.
        const prefs = getPrefs();
        if (prefs.mode === 'search' && prefs.keyword) {
            setMode('search');
            $('#filter-keyword').val(prefs.keyword);
            if (prefs.language !== undefined) $('#filter-language').val(prefs.language);
            loadWall({ mode: 'search', q: prefs.keyword, language: prefs.language });
        } else {
            setMode('headlines');
            if (prefs.country) $('#filter-country').val(prefs.country);
            if (prefs.category) selectCategoryPill(prefs.category);
            loadWall({ mode: 'headlines', country: prefs.country || '', category: prefs.category || '' });
        }
    });
})();
