import { useEffect, useState } from "react";

export function LoadingState({ text = "Loading..." }) {
  return (
    <div className="card shadow-sm border-0">
      <div className="card-body py-5 text-center">
        <div className="spinner-border text-warning mb-3" role="status" />
        <p className="mb-0 text-secondary">{text}</p>
      </div>
    </div>
  );
}

export function EmptyState({ title, description }) {
  return (
    <div className="card shadow-sm border-0">
      <div className="card-body py-5 text-center">
        <h5>{title}</h5>
        <p className="mb-0 text-secondary">{description}</p>
      </div>
    </div>
  );
}

export function ErrorState({ title = "Something went wrong", description, action }) {
  return (
    <div className="card shadow-sm border-0">
      <div className="card-body py-5 text-center">
        <h5>{title}</h5>
        <p className="mb-3 text-secondary">{description}</p>
        {action}
      </div>
    </div>
  );
}

export function ImageWithFallback({ src, fallbackSrc, alt, className, style }) {
  const [currentSrc, setCurrentSrc] = useState(src || fallbackSrc || "");

  useEffect(() => {
    setCurrentSrc(src || fallbackSrc || "");
  }, [src, fallbackSrc]);

  return (
    <img
      className={className}
      style={style}
      src={currentSrc || fallbackSrc}
      alt={alt}
      onError={() => {
        if (fallbackSrc && currentSrc !== fallbackSrc) {
          setCurrentSrc(fallbackSrc);
        }
      }}
    />
  );
}

export function PasswordField({
  label,
  value,
  onChange,
  required,
  placeholder,
  autoComplete = "current-password"
}) {
  const [visible, setVisible] = useState(false);

  return (
    <div>
      <label className="form-label">{label}</label>
      <div className="password-field">
        <input
          className="form-control"
          type={visible ? "text" : "password"}
          required={required}
          placeholder={placeholder}
          autoComplete={autoComplete}
          value={value}
          onChange={onChange}
        />
        <button
          type="button"
          className="password-toggle"
          onClick={() => setVisible((current) => !current)}
          aria-label={visible ? "Hide password" : "Show password"}
          title={visible ? "Hide password" : "Show password"}
        >
          <i className={`bi ${visible ? "bi-eye-slash" : "bi-eye"}`} />
        </button>
      </div>
    </div>
  );
}

export function DashboardBanner({ eyebrow, title, description, imageSrc, children }) {
  return (
    <div className="dashboard-banner">
      <div className="dashboard-banner-copy">
        {eyebrow ? <span className="hero-badge dashboard-kicker">{eyebrow}</span> : null}
        <h2 className="section-title mb-2">{title}</h2>
        <p className="text-secondary mb-0">{description}</p>
        {children ? <div className="dashboard-banner-actions">{children}</div> : null}
      </div>
      <div className="dashboard-banner-visual">
        <ImageWithFallback src={imageSrc} fallbackSrc={imageSrc} alt={title} className="dashboard-banner-image" />
      </div>
    </div>
  );
}

export function StatCard({ label, value, accent = "gold", hint }) {
  return (
    <div className={`stat-card stat-card-${accent}`}>
      <span className="stat-label">{label}</span>
      <strong className="stat-value">{value}</strong>
      {hint ? <small className="text-secondary">{hint}</small> : null}
    </div>
  );
}

export function SectionHeading({ title, subtitle, action }) {
  return (
    <div className="d-flex flex-column flex-lg-row align-items-lg-center justify-content-between gap-3 mb-4">
      <div>
        <h2 className="section-title mb-1">{title}</h2>
        {subtitle ? <p className="text-secondary mb-0">{subtitle}</p> : null}
      </div>
      {action}
    </div>
  );
}

export function DataTable({ columns, rows, emptyMessage = "No records found." }) {
  const safeRows = Array.isArray(rows) ? rows : [];

  if (!safeRows.length) {
    return <EmptyState title="Nothing to show" description={emptyMessage} />;
  }

  return (
    <div className="table-responsive table-shell">
      <table className="table align-middle mb-0">
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column.key}>{column.label}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {safeRows.map((row, index) => (
            <tr key={row.id ?? index}>
              {columns.map((column) => (
                <td key={column.key}>
                  {column.render ? column.render(row) : row[column.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
