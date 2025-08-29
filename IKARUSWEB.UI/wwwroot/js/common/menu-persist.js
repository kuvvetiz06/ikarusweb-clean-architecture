(() => {
    // Controller → (pageKey, accordionKey) eşlemesi
    const ControllerToMenu = {
        // Setup controller (action'a göre farklı sayfalar olabilir ama accordiona "Setup")
        hotel: { pageKey: 'Hotel', accordion: 'Setup' }, 
        roomtype: { pageKey: 'RoomType', accordion: 'Setup' },
        BedType: { pageKey: 'BedTypes', accordion: 'Setup' },
        RoomView: { pageKey: 'RoomViews', accordion: 'Setup' },
        RoomLocation: { pageKey: 'RoomLocations', accordion: 'Setup' },
        RoomDefinition: { pageKey: 'RoomDefinitions', accordion: 'Setup' },
        PensionType: { pageKey: 'PensionTypes', accordion: 'Setup' },
        Nationality: { pageKey: 'Nationalities', accordion: 'Setup' },
        Currency: { pageKey: 'Currencies', accordion: 'Setup' },
        CashRegisterCard: { pageKey: 'CashRegisterCards', accordion: 'Setup' },
        ExpenseCodeDefinition: { pageKey: 'ExpenseCodeDefinitions', accordion: 'Setup' },
        RevenueCodeDefinition: { pageKey: 'RevenueCodeDefinitions', accordion: 'Setup' },
        DepartmentDefinition: { pageKey: 'DepartmentDefinitions', accordion: 'Setup' },
        UserManagement: { pageKey: 'UsersManagement', accordion: 'Setup' },
        QuickPosManagement: { pageKey: 'QuickPosManagement', accordion: 'Setup' },
        HotelConfig: { pageKey: 'HotelConfigs', accordion: 'Setup' },  
        // Diğer menü grupların varsa buraya
        // Reservations: { pageKey: '...', accordion: 'Reservations' },
        // Reports: { pageKey: '...', accordion: 'Reports' },
    };

    // /Area/Controller/Action veya /Controller/Action senaryolarını destekler
    const getControllerFromUrl = () => {
        const parts = window.location.pathname.replace(/\/+$/, '').split('/').filter(Boolean);
        if (parts.length >= 3) {
            // /Area/Controller/Action -> controller = parts[1]
            return parts[1];
        }
        
        // /Controller/Action -> controller = parts[0]
        return parts[0] || 'Dashboard';
    };

    const getMenuConfig = () => {
        const controller = getControllerFromUrl();
        
        // Controller eşleşmesi doğrudan varsa kullan
        if (ControllerToMenu[controller]) return ControllerToMenu[controller];

        // Yoksa href ile tahmin: aynı controller ile başlayan ilk menü linkini bul
        const guess = document.querySelector(`a.menu-link[href^="/${controller}/"]`);
        if (guess) {
            const pageKey = guess.getAttribute('data-page-key') ||
                (guess.id ? guess.id.replace(/_NavLinkItem$/, '') : null);
            const acc = guess.closest('.menu-accordion[id$="_Accordion"]');
            const accordion = acc ? acc.id.replace(/_Accordion$/, '') : null;
            if (pageKey && accordion) return { pageKey, accordion };
        }

        // Fallback: Dashboard
        return { pageKey: 'Dashboard', accordion: 'Dashboard' };
    };

    const clearActive = () => {
        document.querySelectorAll('.menu-link.active, .menu-link.here').forEach(el => el.classList.remove('active', 'here'));
        document.querySelectorAll('.menu-item.here, .menu-item.show').forEach(el => el.classList.remove('here', 'show'));
        document.querySelectorAll('.menu-accordion.show').forEach(el => el.classList.remove('show'));
    };

    const applyActiveByController = () => {
        const { pageKey, accordion } = getMenuConfig();

        clearActive();

        const acc = document.getElementById(`${accordion}_Accordion`);
        acc?.classList.add('show');

        const link = document.getElementById(`${pageKey}_NavLinkItem`) ||
            document.querySelector(`[data-page-key="${pageKey}"]`);
        if (link) {
            link.classList.add('active', 'here');
            link.closest('.menu-item')?.classList.add('here', 'show');
            try { link.scrollIntoView({ block: 'nearest' }); } catch { }
        }
    };

    // İlk yükleme + geri/ileri (BFCache) + history değişimleri
    document.addEventListener('DOMContentLoaded', applyActiveByController);
    window.addEventListener('pageshow', applyActiveByController);
    window.addEventListener('popstate', applyActiveByController);
    window.addEventListener('hashchange', applyActiveByController);

    // Menüye tıklandığında URL değişecek; SPA değilse zaten yeni sayfa yüklenecek.
    // SPA ise burada ekstra routing hook’ları ekleyebilirsin.
})();
