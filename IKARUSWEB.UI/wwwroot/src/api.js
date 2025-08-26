// src/api.js

export async function getOrders({ skip = 0, take = 20, sort = "id", desc = false, search = "" } = {}) {
    const u = new URL("/api/orders", location.origin);
    u.searchParams.set("skip", skip);
    u.searchParams.set("take", take);
    u.searchParams.set("sort", sort);
    u.searchParams.set("desc", desc);
    if (search) u.searchParams.set("search", search);

    const res = await fetch(u, { headers: { "Accept": "application/json" } });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json(); // { data, totalCount }
}
