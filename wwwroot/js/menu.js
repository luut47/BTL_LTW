// wwwroot/js/menu.js
(function () {
    const CART_KEY = 'cart';
    const CUSTOMER_KEY = 'customer';

    function getCart() {
        try {
            return JSON.parse(localStorage.getItem(CART_KEY) || '[]');
        } catch {
            return [];
        }
    }

    function saveCart(cart) {
        localStorage.setItem(CART_KEY, JSON.stringify(cart));
        refreshCartUI();
    }

    function getCustomer() {
        try {
            return JSON.parse(localStorage.getItem(CUSTOMER_KEY) || 'null');
        } catch {
            return null;
        }
    }

    // escape cho innerHTML
    function escapeHtml(s) {
        if (!s) return '';
        return s
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;');
    }

    function refreshCartUI() {
        const cart = getCart();
        const count = cart.reduce((s, i) => s + (i.qty || 0), 0);
        const floatingCount = document.getElementById('floatingCount');
        if (floatingCount) floatingCount.innerText = count;

        const total = cart.reduce((s, i) => s + ((i.unitPrice || 0) * (i.qty || 1)), 0);
        const cartTotal = document.getElementById('cartTotal');
        if (cartTotal) {
            cartTotal.innerText = new Intl.NumberFormat('vi-VN').format(total) + ' VND';
        }

        renderCartItems();
    }

    function renderCartItems() {
        const wrap = document.getElementById('cartItemsWrap');
        if (!wrap) return;

        const cart = getCart();
        wrap.innerHTML = '';
        if (!cart || cart.length === 0) {
            wrap.innerHTML = '<div style="padding:12px;color:#666;">Giỏ trống</div>';
            return;
        }

        cart.forEach((it, idx) => {
            const div = document.createElement('div');
            div.className = 'cart-item-row';
            div.innerHTML = `
                <div class="cart-item-inner">
                  <div>
                    <div class="cart-item-name">${escapeHtml(it.menuItemName || it.name || 'Unknown')}</div>
                    <div class="cart-item-price">
                      ${new Intl.NumberFormat('vi-VN').format(it.unitPrice || it.price || 0)} VND
                    </div>
                  </div>
                  <div class="cart-item-right">
                    <div class="cart-item-qty-wrap">
                      <button class="qty-dec" data-idx="${idx}">−</button>
                      <div class="cart-item-qty">${it.qty || 1}</div>
                      <button class="qty-inc" data-idx="${idx}">+</button>
                    </div>
                    <div class="cart-item-remove">
                      <button class="btn-remove" data-idx="${idx}">Xóa</button>
                    </div>
                  </div>
                </div>
            `;
            wrap.appendChild(div);
        });

        // handlers
        wrap.querySelectorAll('.qty-inc').forEach(b =>
            b.addEventListener('click', e => {
                const i = parseInt(e.currentTarget.dataset.idx);
                const cart = getCart();
                cart[i].qty = (cart[i].qty || 1) + 1;
                saveCart(cart);
            })
        );
        wrap.querySelectorAll('.qty-dec').forEach(b =>
            b.addEventListener('click', e => {
                const i = parseInt(e.currentTarget.dataset.idx);
                const cart = getCart();
                cart[i].qty = Math.max(1, (cart[i].qty || 1) - 1);
                saveCart(cart);
            })
        );
        wrap.querySelectorAll('.btn-remove').forEach(b =>
            b.addEventListener('click', e => {
                const i = parseInt(e.currentTarget.dataset.idx);
                const cart = getCart();
                cart.splice(i, 1);
                saveCart(cart);
            })
        );
    }

    function initAddButtons() {
        document.querySelectorAll('.btn-add').forEach(b => {
            b.addEventListener('click', () => {
                const id = parseInt(b.dataset.id);
                const name = b.dataset.name;
                const price = parseFloat(b.dataset.price) || 0;
                const cart = getCart();
                const found = cart.find(x => x.menuItemId === id);
                if (found) {
                    found.qty = (found.qty || 1) + 1;
                } else {
                    cart.push({
                        menuItemId: id,
                        menuItemName: name,
                        qty: 1,
                        unitPrice: price
                    });
                }
                saveCart(cart);
            });
        });
    }

    function openPanel() {
        const panel = document.getElementById('cartPanel');
        if (panel) panel.style.right = '0';
    }

    function closePanel() {
        const panel = document.getElementById('cartPanel');
        if (panel) panel.style.right = '-420px';
    }

    async function confirmCart() {
        const cart = getCart();
        if (!cart || cart.length === 0) {
            alert('Giỏ rỗng');
            return;
        }

        const cust = getCustomer();
        if (!cust || !cust.name || !cust.phone) {
            alert('Thiếu thông tin khách (hãy quay lại/bấm Chọn đồ lại).');
            return;
        }

        const token = cust.token
            ? cust.token
            : 'C-' + Math.random().toString(36).slice(2, 8).toUpperCase();
        const isTakeaway = document.getElementById('takeawayChk')?.checked || false;
        const reservationId = localStorage.getItem('currentReservationId') || '';

        const payload = {
            tableToken: token,
            customerName: cust.name || '',
            customerPhone: cust.phone || '',
            customerAddress: cust.address || '',
            isTakeAway: isTakeaway,
            ReservationId: reservationId || null,
            items: cart.map(i => ({
                menuItemId: i.menuItemId,
                qty: i.qty || 1,
                note: i.note || ''
            }))
        };

        const btn = document.getElementById('confirmCartBtn');
        try {
            if (btn) {
                btn.disabled = true;
                btn.innerText = 'Đang gửi...';
            }

            const resp = await fetch('/Orders/Create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!resp.ok) {
                const text = await resp.text();
                alert('Tạo order thất bại: ' + text);
                return;
            }

            const data = await resp.json();
            alert('Đã gửi bếp. Mã order: ' + (data.id || 'unknown'));

            localStorage.removeItem('currentReservationId');
            localStorage.removeItem('cart');
            refreshCartUI();
            closePanel();
            window.location.href = '/Home';
        } catch (err) {
            alert('Lỗi gửi order: ' + (err.message || err));
        } finally {
            if (btn) {
                btn.disabled = false;
                btn.innerText = 'Xác nhận món';
            }
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        initAddButtons();

        const openBtn = document.getElementById('openCartBtnFloating');
        const closeBtn = document.getElementById('closeCartBtn');
        const clearBtn = document.getElementById('clearCartBtn');
        const confirmBtn = document.getElementById('confirmCartBtn');

        if (openBtn) openBtn.addEventListener('click', openPanel);
        if (closeBtn) closeBtn.addEventListener('click', closePanel);
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                if (confirm('Xóa toàn bộ giỏ?')) {
                    localStorage.removeItem(CART_KEY);
                    refreshCartUI();
                }
            });
        }
        if (confirmBtn) confirmBtn.addEventListener('click', confirmCart);

        refreshCartUI();
    });
})();
