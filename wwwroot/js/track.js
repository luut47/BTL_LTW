(function () {
    function fetchStatus(orderId) {
        // Ví dụ API: /Orders/GetStatus?id=...
        return fetch('/Orders/GetStatus?id=' + encodeURIComponent(orderId), {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => {
                if (!r.ok) throw new Error('Request failed');
                return r.json();
            });
    }

    function renderStatus(data) {
        // data: { status: "...", items: [...], total: ... } tuỳ bạn trả về
        const statusSpan = document.getElementById('order-status-text');
        if (statusSpan && data.status) {
            statusSpan.textContent = data.status;
        }

        // nếu API trả items, có thể cập nhật luôn
        if (data.items && Array.isArray(data.items)) {
            const ul = document.getElementById('order-items-list');
            if (ul) {
                ul.innerHTML = '';
                data.items.forEach(it => {
                    const li = document.createElement('li');
                    li.textContent = `${it.menuItemName} - x${it.qty}`;
                    ul.appendChild(li);
                });
            }
        }

        if (data.total != null) {
            const totalRow = document.querySelector('.order-total-row strong');
            if (totalRow) {
                totalRow.textContent = 'Tổng: ' +
                    new Intl.NumberFormat('vi-VN').format(data.total) + ' VND';
            }
        }

        const extra = document.getElementById('order-track-extra');
        if (extra && data.status) {
            // ví dụ hiển thị text khác theo status
            const s = data.status.toLowerCase();
            if (s.includes('cooking')) {
                extra.textContent = 'Bếp đang chế biến món của bạn...';
            } else if (s.includes('closed') || s.includes('done')) {
                extra.textContent = 'Đơn đã chuẩn bị xong, vui lòng chờ phục vụ.';
            } else if (s.includes('completed')) {
                extra.textContent = 'Đơn đã hoàn tất. Cảm ơn bạn!';
            } else {
                extra.textContent = '';
            }
        }
    }

    function startPolling(orderId) {
        // gọi 1 lần ngay
        fetchStatus(orderId)
            .then(renderStatus)
            .catch(err => console.error('fetchStatus error:', err));

        // sau đó poll theo chu kỳ, ví dụ 5 giây
        setInterval(() => {
            fetchStatus(orderId)
                .then(renderStatus)
                .catch(err => console.error('fetchStatus error:', err));
        }, 5000);
    }

    document.addEventListener('DOMContentLoaded', () => {
        if (!window.orderTrackConfig || !window.orderTrackConfig.orderId) return;
        const orderId = window.orderTrackConfig.orderId;
        startPolling(orderId);
    });
})();
