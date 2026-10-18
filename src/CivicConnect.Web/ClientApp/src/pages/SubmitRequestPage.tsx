import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api/client";
import type { Category, ProblemDetails } from "../api/types";

const fieldMessage = (errors: Record<string, string[]> | undefined, field: string) =>
  errors?.[field]?.[0] ?? "";

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
      <label className="field">Title <span className="muted">(required, 200 characters maximum)</span>
        <input value={title} maxLength={200} required onChange={(event) => setTitle(event.target.value)} />
        {fieldMessage(errors, "title") && <span className="error">{fieldMessage(errors, "title")}</span>}
      </label>
      <label className="field">Description <span className="muted">(required, 2,000 characters maximum)</span>
        <textarea value={description} maxLength={2000} required rows={7} onChange={(event) => setDescription(event.target.value)} />
        {fieldMessage(errors, "description") && <span className="error">{fieldMessage(errors, "description")}</span>}
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
