import { createContext, useContext, useEffect, useMemo, useState } from "react";

const CartContext = createContext(null);

const STORAGE_KEY = "restaurant_cart";
const EMPTY_ITEMS = [];

export function CartProvider({ children }) {
  const [items, setItems] = useState(() => {
    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return EMPTY_ITEMS;
    }

    try {
      return JSON.parse(raw);
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return EMPTY_ITEMS;
    }
  });

  const normalizeQuantity = (quantity) => {
    const parsedQuantity = Number(quantity);
    return Number.isFinite(parsedQuantity) ? Math.max(Math.floor(parsedQuantity), 1) : 1;
  };

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const addToCart = (menuItem, quantity = 1) => {
    const normalizedQuantity = normalizeQuantity(quantity);
    setItems((current) => {
      const existing = current.find((item) => item.id === menuItem.id);
      if (existing) {
        return current.map((item) =>
          item.id === menuItem.id
            ? {
                ...item,
                name: menuItem.name,
                categoryName: menuItem.categoryName ?? item.categoryName ?? "",
                imageUrl: menuItem.imageUrl ?? item.imageUrl ?? "",
                price: Number(menuItem.finalPrice ?? item.price ?? 0),
                quantity: item.quantity + normalizedQuantity
              }
            : item
        );
      }

      return [
        ...current,
        {
          id: menuItem.id,
          name: menuItem.name,
          categoryName: menuItem.categoryName ?? "",
          imageUrl: menuItem.imageUrl ?? "",
          price: Number(menuItem.finalPrice ?? 0),
          quantity: normalizedQuantity
        }
      ];
    });
  };

  const updateQuantity = (id, quantity) => {
    setItems((current) =>
      current
        .map((item) => (item.id === id ? { ...item, quantity: normalizeQuantity(quantity) } : item))
        .filter((item) => item.quantity > 0)
    );
  };

  const incrementQuantity = (id) => {
    setItems((current) =>
      current.map((item) => (item.id === id ? { ...item, quantity: item.quantity + 1 } : item))
    );
  };

  const decrementQuantity = (id) => {
    setItems((current) =>
      current.flatMap((item) => {
        if (item.id !== id) {
          return item;
        }

        if (item.quantity <= 1) {
          return [];
        }

        return { ...item, quantity: item.quantity - 1 };
      })
    );
  };

  const removeFromCart = (id) => setItems((current) => current.filter((item) => item.id !== id));
  const clearCart = () => setItems([]);

  const value = useMemo(
    () => ({
      items,
      addToCart,
      updateQuantity,
      incrementQuantity,
      decrementQuantity,
      removeFromCart,
      clearCart,
      totalItems: items.reduce((sum, item) => sum + item.quantity, 0),
      totalAmount: items.reduce((sum, item) => sum + item.price * item.quantity, 0)
    }),
    [items]
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart must be used inside CartProvider");
  }

  return context;
}
