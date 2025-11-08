(function () {
    setInterval(reloadResv, 100);

    const btnKitchen = document.getElementById('btnKitchen');
    if (btnKitchen) {
        btnKitchen.addEventListener('click', () => {
            window.location.href = '/Kitchen';
        });
    }
    function reloadResv() {
        fetch('/Staff/ReservationsPartial', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => r.text())
            .then(html => {
                const el = document.getElementById('resv-root');
                if (el) el.innerHTML = html;
            })
            .catch(err => console.error('reloadResv error', err));
    }

    // Bắt click nút "Chọn đồ" (ở Reservations + Staff/Index)
    function handleChooseFoodClick(e) {
        const btn = e.target.closest('.btn-choose-food');
        if (!btn) return;

        e.preventDefault();

        const resid = btn.dataset.id;
        const name = btn.dataset.name || '';
        const phone = btn.dataset.phone || '';
        const email = btn.dataset.email || '';
        const table = btn.dataset.table || '';

        const customer = { name: name, phone: phone, address: email, token: table };
        try {
            localStorage.setItem('customer', JSON.stringify(customer));
            localStorage.setItem('currentReservationId', resid);
            // nếu muốn, xoá giỏ cũ:
            // localStorage.removeItem('cart');
        } catch (err) {
            console.error('localStorage error', err);
        }

        window.location.href = '/Menu';
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Lần đầu + 8s/lần
        reloadResv();
        //setInterval(reloadResv, 100);

        // Event delegation cho nút "Chọn đồ"
        document.addEventListener('click', handleChooseFoodClick);
    });
})();
