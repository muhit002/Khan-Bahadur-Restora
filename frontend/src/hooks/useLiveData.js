import { useEffect, useRef, useState } from "react";
import { DATA_CHANGED_EVENT } from "../utils/liveData";

export function useLiveData(loader, deps = [], options = {}) {
  const {
    enabled = true,
    refreshIntervalMs = 15000,
    initialData = null
  } = options;

  const [data, setData] = useState(initialData);
  const [loading, setLoading] = useState(enabled);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);
  const loaderRef = useRef(loader);

  loaderRef.current = loader;

  const load = async ({ silent = false } = {}) => {
    if (!enabled) {
      setLoading(false);
      return null;
    }

    if (silent) {
      setRefreshing(true);
    } else {
      setLoading(true);
    }

    try {
      const result = await loaderRef.current();
      setData(result);
      setError(null);
      return result;
    } catch (loadError) {
      setError(loadError);
      return null;
    } finally {
      if (silent) {
        setRefreshing(false);
      } else {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    void load();
  }, [enabled, ...deps]);

  useEffect(() => {
    if (!enabled) {
      return undefined;
    }

    const refresh = () => {
      void load({ silent: true });
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState === "visible") {
        refresh();
      }
    };

    window.addEventListener("focus", refresh);
    window.addEventListener(DATA_CHANGED_EVENT, refresh);
    document.addEventListener("visibilitychange", handleVisibilityChange);

    const intervalId =
      refreshIntervalMs > 0 ? window.setInterval(refresh, refreshIntervalMs) : null;

    return () => {
      window.removeEventListener("focus", refresh);
      window.removeEventListener(DATA_CHANGED_EVENT, refresh);
      document.removeEventListener("visibilitychange", handleVisibilityChange);

      if (intervalId) {
        window.clearInterval(intervalId);
      }
    };
  }, [enabled, refreshIntervalMs, ...deps]);

  return {
    data,
    loading,
    refreshing,
    error,
    reload: load
  };
}
