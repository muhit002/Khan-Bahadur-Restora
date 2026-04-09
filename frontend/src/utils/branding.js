export const brand = {
  name: "Khan Bahadur Restora",
  label: "খান বাহাদুর রেস্টোরা",
  tagline: "ঐতিহ্যের স্বাদ, সেবার শৃঙ্খলা",
  promise: "Classic dining warmth with a modern restaurant workflow."
};

export const dashboardBanners = {
  home: "/brand/banners/home-banner.svg",
  admin: "/brand/banners/admin-dashboard.svg",
  manager: "/brand/banners/manager-dashboard.svg",
  customer: "/brand/banners/customer-dashboard.svg"
};

const categoryFallbacks = {
  Starters: "/brand/categories/starters.svg",
  "Main Course": "/brand/categories/main-course.svg",
  Desserts: "/brand/categories/desserts.svg",
  Drinks: "/brand/categories/drinks.svg"
};

const menuFallbacks = {
  "Smoked Chicken Wings": "/brand/foods/chicken-wings.svg",
  "Grilled River Fish": "/brand/foods/grilled-fish.svg",
  "Beef Steak Platter": "/brand/foods/beef-steak.svg",
  "Molten Lava Cake": "/brand/foods/lava-cake.svg",
  "Classic Cold Coffee": "/brand/foods/cold-coffee.svg"
};

export function getCategoryFallback(categoryName) {
  return categoryFallbacks[categoryName] ?? "/brand/categories/default-category.svg";
}

export function getMenuFallback(item) {
  if (!item) {
    return "/brand/foods/default-dish.svg";
  }

  if (item.name && menuFallbacks[item.name]) {
    return menuFallbacks[item.name];
  }

  if (item.categoryName && categoryFallbacks[item.categoryName]) {
    return categoryFallbacks[item.categoryName];
  }

  return "/brand/foods/default-dish.svg";
}
