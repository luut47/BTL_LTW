(function () {
    // Hàm load lại danh sách đơn bếp
    function reloadKitchen() {
        fetch('/Kitchen/KitchenPartial', {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => r.text())
            .then(html => {
                const root = document.getElementById('kitchen-root');
                if (root) root.innerHTML = html;
                attachOrderButtons();
            })
            .catch(err => console.error('reloadKitchen error', err));
    }

    // Gắn handler cho các nút trong partial
    function attachOrderButtons() {
        // nút Bắt đầu
        document.querySelectorAll('.btn-start').forEach(btn => {
            btn.addEventListener('click', function () {
                const id = this.dataset.id;
                if (!id) return;
                updateStatus(id, 'cooking'); // ví dụ
            });
        });

        // nút Done / Closed
        document.querySelectorAll('.btn-done').forEach(btn => {
            btn.addEventListener('click', function () {
                const id = this.dataset.id;
                if (!id) return;
                updateStatus(id, 'closed'); // status mà bạn dùng trong DB
            });
        });

        // nút Hủy
        document.querySelectorAll('.btn-cancel').forEach(btn => {
            btn.addEventListener('click', function () {
                const id = this.dataset.id;
                if (!id) return;
                updateStatus(id, 'cancelled');
            });
        });
    }

    // Gọi API cập nhật trạng thái đơn bếp
    function updateStatus(orderId, newStatus) {
        fetch('/Kitchen/UpdateStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ id: orderId, status: newStatus })
        })
            .then(r => {
                if (!r.ok) throw new Error('Request failed');
                return r.json();
            })
            .then(data => {
                // Sau khi update, reload lại danh sách
                reloadKitchen();
            })
            .catch(err => {
                console.error('updateStatus error', err);
                alert('Cập nhật trạng thái thất bại');
            });
    }

    // Khởi động khi DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        reloadKitchen();
         setInterval(reloadKitchen, 5000);
    });
})();
