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

    // ==== xử lý form đặt bàn qua fetch (Reservation/Create) ====
    const formReserve = document.getElementById('reserveForm');
    if (formReserve) {
        const dt = formReserve.querySelector('input[name="DateTime"]');
        if (dt && !dt.value) {
            const now = new Date();
            now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
            dt.value = now.toISOString().slice(0, 16);
        }

        formReserve.addEventListener('submit', async (e) => {
            e.preventDefault();

            // HTML5 validation cho form đặt bàn
            if (!formReserve.reportValidity()) {
                return;
            }

            const fd = new FormData(formReserve);
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
                formReserve.reset();
            } catch (err) {
                alert('Lỗi: ' + (err?.message || err));
            }
        });
    }

    // ==== nút Xem Menu & Đặt món (Order bình thường) ====
    const btnMenu = document.getElementById('goMenuBtn');
    const quickForm = document.getElementById('custQuickForm');

    if (btnMenu && quickForm) {
        btnMenu.addEventListener('click', () => {
            // RẤT QUAN TRỌNG: chạy HTML5 validation trên form nhanh
            if (!quickForm.reportValidity()) {
                // Nếu có field sai (required/pattern), browser sẽ highlight và không cho đi tiếp
                return;
            }

            const name = document.getElementById('custName').value.trim();
            const phone = document.getElementById('custPhone').value.trim();
            const addr = document.getElementById('custAddress').value.trim();


            // (Optional) backup check JS, phòng khi browser cũ không hỗ trợ pattern
            const nameRegex = /^[\p{L}\s]+$/u;
            if (!nameRegex.test(name)) {
                alert('Tên chỉ được bao gồm chữ cái và khoảng trắng.');
                return;
            }

            const phoneRegex = /^0\d{9}$/;
            if (!phoneRegex.test(phone)) {
                alert('Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.');
                return;
            }

            const token = genToken(5);
            const customer = { name, phone, address: addr, token };
            localStorage.setItem('customer', JSON.stringify(customer));

            window.location.href = '/Menu';
        });
    }
});
