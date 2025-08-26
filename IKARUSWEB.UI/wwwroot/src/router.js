// src/router.js

export class Router {
    constructor({ routes, onNavigate }) {
        this.routes = routes; // [{ path: "/orders/:id?", loader: () => new Comp() }]
        this.onNavigate = onNavigate;
        window.addEventListener("hashchange", () => this._handle());
        this._handle(); // first
    }

    _parse(pathPattern, hashPath) {
        // çok basit bir parser: /orders/:id? gibi
        const pSeg = pathPattern.split("/").filter(Boolean);
        const hSeg = hashPath.split("/").filter(Boolean);
        let params = {};
        for (let i = 0; i < pSeg.length; i++) {
            const p = pSeg[i], h = hSeg[i];
            if (!p) continue;
            if (p.startsWith(":")) {
                const optional = p.endsWith("?");
                const key = p.replace(/^:/, "").replace(/\?$/, "");
                if (h !== undefined) params[key] = decodeURIComponent(h);
                else if (!optional) return null;
            } else if (p !== h) {
                return null;
            }
        }
        // fazla segment varsa kabul etmeyelim
        if (hSeg.length > pSeg.length) return null;
        return params;
    }

    _handle() {
        const hash = location.hash.replace(/^#/, "") || "/";
        for (const r of this.routes) {
            const params = this._parse(r.path, hash);
            if (params) {
                this.onNavigate(r, params);
                return;
            }
        }
        // not found → ilk route
        this.onNavigate(this.routes[0], {});
    }
}
