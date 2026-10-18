import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import type { Category } from "../api/types";

export default function ManagerCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [editingId, setEditingId] = useState<number>();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [status, setStatus] = useState("");

  const load = async () => {
    try { setCategories((await api.get<Category[]>("/categories")).data); }
    catch { setError("Could not load categories."); }
  };
  useEffect(() => { void load(); }, []);

  const save = async (event: React.FormEvent) => {
    event.preventDefault(); setBusy(true); setError(""); setStatus("");
    try {
      const data = { name: name.trim(), description: description.trim() };
      if (editingId === undefined) await api.post("/management/categories", data);
      else await api.put(`/management/categories/${editingId}`, data);
      setName(""); setDescription(""); setEditingId(undefined);
      await load(); setStatus("Category saved.");
    } catch { setError("Could not save category. Check name for duplicates."); }
    finally { setBusy(false); }
  };

  return <section className="page">
    <Link className="back" to="/manager/dashboard">← Dashboard</Link><h1>Categories</h1>
    {error && <p className="error" role="alert">{error}</p>}{status && <p role="status">{status}</p>}
    <section className="card"><h2>{editingId === undefined ? "Create category" : "Edit category"}</h2><form onSubmit={(event) => { void save(event); }} className="filters">
      <label>Name<input required value={name} maxLength={200} onChange={(event) => setName(event.target.value)} /></label>
      <label>Description<textarea value={description} onChange={(event) => setDescription(event.target.value)} /></label>
      <button className="btn-primary" disabled={busy}>{editingId === undefined ? "Create" : "Save"}</button>
      {editingId !== undefined && <button type="button" className="btn-secondary" onClick={() => { setEditingId(undefined); setName(""); setDescription(""); }}>Cancel edit</button>}
    </form></section>
    <section className="card"><h2>Existing categories</h2><ul>{categories.map((item) => <li key={item.id}>{item.name} — {item.description} <button className="btn-link" onClick={() => { setEditingId(item.id); setName(item.name); setDescription(item.description); setStatus(""); }}>Edit {item.name}</button></li>)}</ul></section>
  </section>;
}
