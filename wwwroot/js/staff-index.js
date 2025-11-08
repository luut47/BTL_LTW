// wwwroot/js/staff-index.js
(function () {
    let reloading = false;
    setInterval(reloadOrders, 5000);
    function reloadOrders() {
        if (reloading) return;
        fetch('/Staff/OrdersPartial', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => r.text())
            .then(html => {
                const root = document.getElementById('orders-root');
                if (root) root.innerHTML = html;
            })
            .catch(err => console.error('reloadOrders error', err));
    }

    document.addEventListener('DOMContentLoaded', function () {
        // lần đầu
        reloadOrders();
        // sau đó 5s/lần (tùy bạn chỉnh)
        
    });
})();
