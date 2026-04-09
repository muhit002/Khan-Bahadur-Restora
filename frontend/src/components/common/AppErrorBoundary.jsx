import React from "react";
import { Link } from "react-router-dom";
import { brand } from "../../utils/branding";

export default class AppErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, errorMessage: "" };
  }

  static getDerivedStateFromError(error) {
    return {
      hasError: true,
      errorMessage: error?.message ?? "An unexpected display error occurred."
    };
  }

  componentDidCatch(error) {
    console.error("App render error:", error);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="centered-shell px-3">
          <div className="auth-card text-center">
            <span className="hero-badge mb-3">{brand.label}</span>
            <h1 className="section-title mb-3">This page could not load</h1>
            <p className="text-secondary mb-3">
              {brand.name} hit a display error. The app is still running, but this screen needs to be reloaded safely.
            </p>
            <p className="small text-secondary mb-4">{this.state.errorMessage}</p>
            <div className="d-flex justify-content-center gap-2 flex-wrap">
              <Link to="/" className="btn btn-outline-dark">
                Back Home
              </Link>
              <button type="button" className="btn btn-warning" onClick={() => window.location.reload()}>
                Reload Page
              </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
