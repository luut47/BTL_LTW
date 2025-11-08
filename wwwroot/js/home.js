// tạo token ngắn (C-XXXXX)
function genToken(len = 5) {
    const chars = 'ABCDEFGHJKMNPQRSTUVWXYZ23456789';
    let s = '';
    for (let i = 0; i < len; i++) s += chars.charAt(Math.floor(Math.random() * chars.length));
    return 'C-' + s;
}

document.addEventListener('DOMContentLoaded', () => {
    const modal = document.getElementById('reserveModal');
    const btnOpen = document.getElementById('btnReserve');
    const btnClose = document.getElementById('btnReserveCancel');
    const btnWaiter = document.getElementById('btnWaiter');

    if (btnOpen) {
        btnOpen.addEventListener('click', () => {
            if (modal) modal.style.display = 'flex';
        });
    }

    if (btnClose) {
        btnClose.addEventListener('click', () => {
            if (modal) modal.style.display = 'none';
        });
    }

    if (btnWaiter) {
        btnWaiter.addEventListener('click', () => {
            window.location.href = '/Staff';
        });
    }

    // ==== xử lý form đặt bàn qua fetch (đúng URL /Reservation/Create) ====
    const form = document.getElementById('reserveForm');
    if (form) {
        const dt = form.querySelector('input[name="DateTime"]');
        if (dt && !dt.value) {
            const now = new Date();
            now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
            dt.value = now.toISOString().slice(0, 16);
        }

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const fd = new FormData(form);
            try {
                const resp = await fetch('/Reservation/Create', {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    body: fd
                });
                if (!resp.ok) throw new Error(await resp.text() || 'Gửi thất bại');

                const js = await resp.json();
                alert('Đặt bàn thành công! Mã: ' + js.id);
                if (modal) modal.style.display = 'none';
                form.reset();
            } catch (err) {
                alert('Lỗi: ' + (err?.message || err));
            }
        });
    }

    // ==== nút Xem Menu & Đặt món ====
    const btnMenu = document.getElementById('goMenuBtn');
    if (btnMenu) {
        btnMenu.addEventListener('click', () => {
            const name = document.getElementById('custName').value.trim();
            const phone = document.getElementById('custPhone').value.trim();
            const addr = document.getElementById('custAddress').value.trim();

            if (!name || !phone) {
                alert('Vui lòng nhập tên và số điện thoại');
                return;
            }

            const token = genToken(5);
            const customer = { name, phone, address: addr, token };
            localStorage.setItem('customer', JSON.stringify(customer));

            window.location.href = '/Menu';
        });
    }
});
