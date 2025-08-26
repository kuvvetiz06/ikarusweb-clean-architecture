// src/app.js
import { mount } from "./framework.js";
import { Router } from "./router.js";
import { OrdersPage } from "./components/OrdersPage.js";

const container = document.getElementById("app");

// basit ana sayfa
class Home {
    constructor() { this._mounted = false; }
    _mount(c) { this._mounted = true; c.innerHTML = `<p>Merhaba 👋 Mini framework’e hoş geldin.</p>`; }
    _unmount() { this._mounted = false; }
}

let current = null;
function show(comp) {
    // aktif component’i temizle
    if (current && current._unmount) current._unmount();

    // blazor-like component ise mount(); değilse basit class
    if (comp.render) {
        const un = mount(comp, container);
        current = { _unmount: un };
    } else {
        comp._mount(container);
        current = comp;
    }
}

const router = new Router({
    routes: [
        { path: "/", loader: () => new Home() },
        { path: "/orders", loader: () => new OrdersPage() },
    ],
    onNavigate: (route, params) => {
        const comp = route.loader(params);
        show(comp);
    }
});

// ilk görüntüleme
