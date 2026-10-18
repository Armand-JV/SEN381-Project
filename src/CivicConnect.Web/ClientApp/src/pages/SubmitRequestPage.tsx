import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api/client";
import type { Category, ProblemDetails } from "../api/types";

const TITLE_MAX = 200;
const DESCRIPTION_MAX = 2000;

const fieldMessage = (errors: Record<string, string[]> | undefined, field: string) =>
  errors?.[field]?.[0] ?? "";

function charClass(value: string, max: number) {
  const len = value.length;
  if (len === 0) return "muted";
  if (len >= max) return "error";
  if (len > max * 0.8) return "warn";
  return "";
}

export default function SubmitRequestPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const clearTitleError = () => {
    if (errors.title) setErrors((previous) => { const copy = { ...previous }; delete copy.title; return copy; });
  };
  const clearDescriptionError = () => {
    if (errors.description) setErrors((previous) => { const copy = { ...previous }; delete copy.description; return copy; });
  };

  useEffect(() => {
    let active = true;
    api.get<Category[]>("/categories")
      .then((response) => { if (active) setCategories(response.data); })
      .catch(() => { if (active) setError("Could not load categories."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setErrors({});
    setError("");
    setBusy(true);
    try {
      const response = await api.post<{ id: number }>("/requests", {
        title: title.trim(), description: description.trim(), categoryId: Number(categoryId)
      });
      navigate(`/requests/${response.data.id}`);
    } catch (cause) {
      const response = (cause as { response?: { data?: ProblemDetails } }).response;
      const problem = response?.data;
      if (problem?.errors) setErrors(problem.errors);
      else setError(problem?.detail || problem?.title || "Could not submit request.");
    } finally { setBusy(false); }
  };

  return <section className="page">
    <Link className="back" to="/my-requests">← My requests</Link>
    <h1>Submit Request</h1>
    <p className="muted">Describe a civic issue for the municipality to review.</p>
    {error && <p className="error" role="alert">{error}</p>}
    <form className="card" onSubmit={submit} noValidate>
      <label className="field">Title <span className={`muted ${charClass(title, TITLE_MAX)}`}>({title.length}/{TITLE_MAX})</span>
        <input
          value={title}
          required
          maxLength={TITLE_MAX}
          onChange={(event) => { setTitle(event.target.value); clearTitleError(); }}
          aria-invalid={!!errors.title}
        />
        {fieldMessage(errors, "title") && <span className="error" role="alert">{fieldMessage(errors, "title")}</span>}
      </label>
      <label className="field">Description <span className={`muted ${charClass(description, DESCRIPTION_MAX)}`}>({description.length}/{DESCRIPTION_MAX})</span>
        <textarea
          value={description}
          required
          rows={7}
          maxLength={DESCRIPTION_MAX}
          onChange={(event) => { setDescription(event.target.value); clearDescriptionError(); }}
          aria-invalid={!!errors.description}
        />
        {fieldMessage(errors, "description") && <span className="error" role="alert">{fieldMessage(errors, "description")}</span>}
      </label>
      <label className="field">Category
        <select value={categoryId} required disabled={loading} onChange={(event) => setCategoryId(event.target.value)}>
          <option value="">Choose category</option>
          {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
        </select>
        {fieldMessage(errors, "categoryId") && <span className="error">{fieldMessage(errors, "categoryId")}</span>}
      </label>
      <button className="btn-primary" disabled={busy || loading || categories.length === 0}>{busy ? "Submitting…" : "Submit request"}</button>
    </form>
  </section>;
}
