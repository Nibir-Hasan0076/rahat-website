document.addEventListener("DOMContentLoaded", () => {
  const money = new Intl.NumberFormat("en-US", { maximumFractionDigits: 2 });
  const formatTk = (value) => "৳" + money.format(value || 0);

  // Dismissible, auto-hiding flash messages.
  document.querySelectorAll(".flash").forEach((el) => {
    const close = el.querySelector("button");
    if (close) {
      close.addEventListener("click", () => el.remove());
    }
    if (el.dataset.autohide !== undefined) {
      setTimeout(() => {
        el.classList.add("hide");
        setTimeout(() => el.remove(), 280);
      }, 4500);
    }
  });

  // Quantity steppers.
  document.querySelectorAll("[data-qty]").forEach((stepper) => {
    const input = stepper.querySelector('input[type="number"]');
    if (!input) return;

    stepper.querySelectorAll("button[data-step]").forEach((button) => {
      button.addEventListener("click", () => {
        const step = parseInt(button.dataset.step, 10) || 0;
        const min = parseInt(input.min || "1", 10);
        const max = parseInt(input.max || "9999", 10);
        let value = (parseInt(input.value, 10) || min) + step;
        value = Math.min(Math.max(value, min), max);
        input.value = value;
        input.dispatchEvent(new Event("change", { bubbles: true }));
      });
    });
  });

  // Product page: reflect the chosen quantity in the add-to-bag price.
  document.querySelectorAll("[data-price-form]").forEach((form) => {
    const unit = parseFloat(form.dataset.unitPrice);
    const input = form.querySelector('input[name="quantity"]');
    const total = form.querySelector("[data-price-total]");
    if (!input || !total || Number.isNaN(unit)) return;

    const update = () => {
      const quantity = parseInt(input.value, 10) || 1;
      total.textContent = formatTk(unit * quantity);
    };
    input.addEventListener("change", update);
    update();
  });

  // Cart: live totals plus a debounced save so checkout always sees the new quantity.
  const cart = document.querySelector("[data-cart]");
  if (cart) {
    const fee = parseFloat(cart.dataset.shippingFee || "0");
    const threshold = parseFloat(cart.dataset.freeThreshold || "0");
    const pendingSaves = new Set();
    const timers = new WeakMap();

    const updateTotals = () => {
      let subtotal = 0;
      let count = 0;

      cart.querySelectorAll(".cart-row").forEach((row) => {
        const price = parseFloat(row.dataset.unitPrice || "0");
        const input = row.querySelector('input[name="quantity"]');
        const quantity = parseInt(input ? input.value : "0", 10) || 0;
        subtotal += price * quantity;
        count += quantity;

        const line = row.querySelector("[data-line-total]");
        if (line) line.textContent = formatTk(price * quantity);
      });

      const shipping = subtotal >= threshold ? 0 : fee;
      const setText = (selector, text) => {
        const el = cart.querySelector(selector);
        if (el) el.textContent = text;
      };

      setText("[data-summary-count]", String(count));
      setText("[data-summary-count-word]", count === 1 ? "item" : "items");
      setText("[data-summary-subtotal]", formatTk(subtotal));
      setText("[data-summary-delivery]", shipping === 0 ? "Free" : formatTk(shipping));
      setText("[data-summary-total]", formatTk(subtotal + shipping));

      const note = cart.querySelector("[data-free-note]");
      if (note) {
        note.classList.toggle("unlocked", shipping === 0);
        if (shipping === 0) {
          note.textContent = "You've unlocked free delivery on this order.";
        } else {
          note.innerHTML = "Add <strong>" + formatTk(threshold - subtotal) + "</strong> more to unlock free delivery.";
        }
      }

      const badge = document.querySelector("[data-bag-count]");
      if (badge) badge.textContent = String(count);
    };

    const saveQuantity = async (form) => {
      clearTimeout(timers.get(form));
      timers.delete(form);
      const body = new URLSearchParams(new FormData(form));
      try {
        await fetch(form.action, {
          method: "POST",
          body,
          credentials: "same-origin"
        });
      } catch {
        // Keep the optimistic UI; the next update or checkout will resync.
      } finally {
        pendingSaves.delete(form);
      }
    };

    cart.querySelectorAll(".cart-row").forEach((row) => {
      const input = row.querySelector('input[name="quantity"]');
      const form = row.querySelector('form[action$="/Cart/Update"]');
      if (!input) return;

      if (form) {
        form.addEventListener("submit", () => {
          clearTimeout(timers.get(form));
          pendingSaves.delete(form);
        });
      }

      input.addEventListener("change", () => {
        updateTotals();
        if (!form) return;
        clearTimeout(timers.get(form));
        pendingSaves.add(form);
        timers.set(form, setTimeout(() => saveQuantity(form), 450));
      });
    });

    const checkoutLink = document.querySelector("[data-checkout-link]");
    if (checkoutLink) {
      checkoutLink.addEventListener("click", async (event) => {
        if (pendingSaves.size === 0) return;
        event.preventDefault();
        const forms = Array.from(pendingSaves);
        await Promise.all(forms.map((form) => saveQuantity(form)));
        window.location.href = checkoutLink.href;
      });
    }
  }

  // Product gallery thumbnails.
  document.querySelectorAll("[data-gallery]").forEach((gallery) => {
    const main = document.getElementById("productMainImage");
    if (!main) return;

    gallery.querySelectorAll("[data-gallery-src]").forEach((thumb) => {
      thumb.addEventListener("click", () => {
        main.src = thumb.dataset.gallerySrc;
        gallery.querySelectorAll(".detail-thumb").forEach((item) => item.classList.remove("active"));
        thumb.classList.add("active");
      });
    });
  });

  // Live image previews on admin upload forms.
  document.querySelectorAll('input[type="file"][data-preview]').forEach((input) => {
    input.addEventListener("change", () => {
      const preview = document.querySelector(input.dataset.preview);
      const file = input.files && input.files[0];
      if (!preview || !file) return;
      preview.src = URL.createObjectURL(file);
      preview.hidden = false;
    });
  });

  // Destructive-action confirmation.
  document.querySelectorAll("form[data-confirm]").forEach((form) => {
    form.addEventListener("submit", (event) => {
      if (!window.confirm(form.dataset.confirm)) {
        event.preventDefault();
      }
    });
  });
});
