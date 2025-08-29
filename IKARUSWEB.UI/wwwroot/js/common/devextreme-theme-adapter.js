
(function () {
    // DevExtreme tema yolunu moda göre üret
    function dxHrefFor(mode) {
        return mode === 'dark'
            ? '/css/devextreme/dx.material.blue.dark.compact.css'
            : '/css/devextreme/dx.material.blue.light.compact.css';
    }

    // Güvenli tema değişimi (preload + onload)
    function applyDxTheme(mode) {
        var desired = dxHrefFor(mode);
        var link = document.getElementById('dx-theme');

        if (!link) {
            link = document.createElement('link');
            link.id = 'dx-theme';
            link.rel = 'stylesheet';
            document.head.appendChild(link);
        }

        // Aynı dosyaysa hiçbir şey yapma
        if (link.href && link.href.indexOf(desired) !== -1) return;

        // Geçişte kısa süreyle DevExtreme container’ını sakla (FOUC guard)
        document.documentElement.classList.add('dx-pending');

        // Preload tekniğiyle yeni CSS’i arka planda al
        var preload = document.createElement('link');
        preload.rel = 'preload';
        preload.as = 'style';
        preload.href = desired;

        preload.onload = function () {
            // Yeni tema dosyası hazır; tek link’i güncelle
            link.href = desired;

            // Uygulandıktan hemen sonra görünür yap
            document.documentElement.classList.remove('dx-pending');
            preload.remove();
        };

        // Aksilikte kilitli kalmasın
        preload.onerror = function () {
            document.documentElement.classList.remove('dx-pending');
            preload.remove();
        };

        document.head.appendChild(preload);
    }

    // İlk yükte mevcut modu yakala
    function getCurrentMode() {
        try {
            // Metronic API’si varsa onu kullan
            if (window.KTThemeMode && typeof KTThemeMode.getMode === 'function') {
                return KTThemeMode.getMode();
            }
        } catch (e) { }
        // Fallback
        var attr = document.documentElement.getAttribute('data-bs-theme');
        if (attr) return attr;
        var ls = localStorage.getItem('data-bs-theme');
        return ls || 'light';
    }

    // Başlangıçta bir kere uygula
    applyDxTheme(getCurrentMode());

    // Tema değişimlerini dinle (Metronic event’i)
    var handler = function () { applyDxTheme(getCurrentMode()); };

    if (window.KTThemeMode && typeof KTThemeMode.on === 'function') {
        // Metronic'in event handler’ı
        KTThemeMode.on('kt.thememode.change', handler);
    } else {
        // Güvenli alternatif: CustomEvent yakala
        document.documentElement.addEventListener('kt.thememode.change', handler);
    }
})();

