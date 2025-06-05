import tkinter as tk
from tkinter import ttk, messagebox, scrolledtext, simpledialog
import json
from utils import (
    load_loot_items,
    load_all_tags,
    load_presets,
    save_presets,
    generate_loot,
    LootItem,
    _resolve,
)


class LootGeneratorApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Loot Generator")
        self.loot_items = load_loot_items()
        self.all_tags = load_all_tags()
        self.presets = load_presets()
        self.setup_ui()
        self.update_preset_listbox()

    def setup_ui(self):
        frame = ttk.Frame(self.root, padding="10")
        frame.pack(fill=tk.BOTH, expand=True)

        # Loot Points & Tags input
        ttk.Label(frame, text="Loot Points:").grid(row=0, column=0, sticky=tk.W)
        self.loot_points_entry = ttk.Entry(frame)
        self.loot_points_entry.grid(row=0, column=1, sticky=tk.EW)

        ttk.Label(frame, text="Include Tags (comma-separated):").grid(row=1, column=0, sticky=tk.W)
        self.include_tags_entry = ttk.Entry(frame)
        self.include_tags_entry.grid(row=1, column=1, sticky=tk.EW)

        ttk.Label(frame, text="Exclude Tags (comma-separated):").grid(row=2, column=0, sticky=tk.W)
        self.exclude_tags_entry = ttk.Entry(frame)
        self.exclude_tags_entry.grid(row=2, column=1, sticky=tk.EW)

        # Rarities
        ttk.Label(frame, text="Max Rarity (numeric):").grid(row=3, column=0, sticky=tk.W)
        self.max_rarity_entry = ttk.Entry(frame)
        self.max_rarity_entry.grid(row=3, column=1, sticky=tk.EW)

        ttk.Label(frame, text="Min Rarity (numeric):").grid(row=4, column=0, sticky=tk.W)
        self.min_rarity_entry = ttk.Entry(frame)
        self.min_rarity_entry.grid(row=4, column=1, sticky=tk.EW)


        #Generate button
        ttk.Button(frame, text="Generate Loot", command=self.generate_loot).grid(row=5, column=0, columnspan=2, pady=5)

        # Presets management - searchable listbox
        ttk.Label(frame, text="Search Presets:").grid(row=6, column=0, sticky=tk.W)
        self.preset_search_var = tk.StringVar()
        self.preset_search_var.trace_add("write", lambda *args: self.update_preset_listbox())
        ttk.Entry(frame, textvariable=self.preset_search_var).grid(row=6, column=1, sticky=tk.EW)

        ttk.Label(frame, text="Presets:").grid(row=7, column=0, sticky=tk.W)
        self.preset_listbox = tk.Listbox(frame, height=5)
        self.preset_listbox.grid(row=7, column=1, sticky=tk.EW)

        ttk.Button(frame, text="Load Preset", command=self.load_preset).grid(row=8, column=0, columnspan=2)
        ttk.Button(frame, text="Save Preset", command=self.save_preset).grid(row=9, column=0, columnspan=2)
        ttk.Button(frame, text="Delete Preset", command=self.delete_preset).grid(row=10, column=0, columnspan=2)

        # Loot display
        ttk.Label(frame, text="Generated Loot:").grid(row=11, column=0, sticky=tk.W)
        self.output_area = scrolledtext.ScrolledText(frame, height=8)
        self.output_area.grid(row=12, column=0, columnspan=2, sticky=tk.NSEW, pady=5)

        # Add/Delete Items
        ttk.Button(frame, text="Add Item", command=self.add_item).grid(row=13, column=0, pady=5)
        ttk.Button(frame, text="Delete Item", command=self.delete_item).grid(row=13, column=1, pady=5)

        ttk.Button(frame, text="Show Tags", command=self.show_tags).grid(row=14, column=0, columnspan=2, pady=5)

        frame.columnconfigure(1, weight=1)
        frame.rowconfigure(12, weight=1)


    def generate_loot(self):
        points = int(self.loot_points_entry.get())
        include_tags = [tag.strip() for tag in self.include_tags_entry.get().split(',')] if self.include_tags_entry.get() else None
        exclude_tags = [tag.strip() for tag in self.exclude_tags_entry.get().split(',')] if self.exclude_tags_entry.get() else None
        min_rarity = int(self.min_rarity_entry.get()) if self.min_rarity_entry.get() else None
        max_rarity = int(self.max_rarity_entry.get()) if self.max_rarity_entry.get() else None

        loot = generate_loot(
            self.loot_items,
            points,
            include_tags,
            exclude_tags,
            min_rarity,
            max_rarity,
        )

        self.output_area.delete('1.0', tk.END)
        if loot:
            item_counts = {}
            for item in loot:
                if item.name in item_counts:
                    item_counts[item.name]["count"] += 1
                else:
                    item_counts[item.name] = {"item": item, "count": 1}

            for data in item_counts.values():
                item = data["item"]
                count = data["count"]
                tags_str = ", ".join(item.tags)
                if count > 1:
                    self.output_area.insert(
                        tk.END,
                        f"{count}x {item.name} (Rarity: {item.rarity}, Tags: {tags_str}) - {item.description} [{item.point_value} points each]\n",
                    )
                else:
                    self.output_area.insert(
                        tk.END,
                        f"{item.name} (Rarity: {item.rarity}, Tags: {tags_str}) - {item.description} [{item.point_value} points]\n",
                    )
        else:
            self.output_area.insert(tk.END, "No loot items matched your criteria.\n")


    def load_preset(self):
        selection = self.preset_listbox.curselection()
        if not selection:
            messagebox.showerror("Error", "Preset not selected.")
            return
        preset_name = self.preset_listbox.get(selection[0])
        preset = self.presets.get(preset_name)
        if preset:
            self.loot_points_entry.delete(0, tk.END)
            self.loot_points_entry.insert(0, str(preset['loot_points']))
            include_tags = preset.get('include_tags', preset.get('tags', []))
            exclude_tags = preset.get('exclude_tags', [])
            self.include_tags_entry.delete(0, tk.END)
            self.include_tags_entry.insert(0, ', '.join(include_tags))
            self.exclude_tags_entry.delete(0, tk.END)
            self.exclude_tags_entry.insert(0, ', '.join(exclude_tags))
        else:
            messagebox.showerror("Error", "Preset not found.")

    def save_preset(self):
        preset_name = simpledialog.askstring("Save Preset", "Preset Name:")
        if preset_name:
            points = int(self.loot_points_entry.get())
            include_tags = [tag.strip() for tag in self.include_tags_entry.get().split(',') if tag.strip()]
            exclude_tags = [tag.strip() for tag in self.exclude_tags_entry.get().split(',') if tag.strip()]

            self.presets[preset_name] = {
                "loot_points": points,
                "include_tags": include_tags,
                "exclude_tags": exclude_tags,
            }
            save_presets(self.presets)
            self.update_preset_listbox()
            messagebox.showinfo("Saved", f"Preset '{preset_name}' saved successfully!")

    def delete_preset(self):
        selection = self.preset_listbox.curselection()
        if selection:
            preset_name = self.preset_listbox.get(selection[0])
        else:
            preset_name = None
        if preset_name in self.presets:
            if messagebox.askyesno("Delete", f"Are you sure you want to delete '{preset_name}'?"):
                del self.presets[preset_name]
                save_presets(self.presets)
                self.update_preset_listbox()
                messagebox.showinfo("Deleted", f"Preset '{preset_name}' deleted.")
        else:
            messagebox.showerror("Error", "Preset not found.")

    def add_item(self):
        add_window = tk.Toplevel(self.root)
        add_window.title("Add New Loot Item")

        fields = ["Name", "Rarity (numeric, higher is rarer)", "Description", "Point Value", "Tags (comma-separated)"]
        entries = {}

        for idx, field in enumerate(fields):
            ttk.Label(add_window, text=field).grid(row=idx, column=0, sticky=tk.W, pady=2)
            entry = ttk.Entry(add_window, width=40)
            entry.grid(row=idx, column=1, pady=2)
            entries[field] = entry

        def save_new_item():
            try:
                item = LootItem(
                    name=entries["Name"].get(),
                    rarity=int(entries["Rarity (numeric, higher is rarer)"].get()),
                    description=entries["Description"].get(),
                    point_value=int(entries["Point Value"].get()),
                    tags=[tag.strip() for tag in entries["Tags (comma-separated)"].get().split(',')]
                )
                self.loot_items.append(item)
                self.update_loot_file()
                messagebox.showinfo("Success", f"Item '{item.name}' added.")
                add_window.destroy()
            except Exception as e:
                messagebox.showerror("Error", f"Invalid input: {e}")

        ttk.Button(add_window, text="Add Item", command=save_new_item).grid(row=len(fields), column=0, columnspan=2, pady=5)

    def delete_item(self):
        item_names = [item.name for item in self.loot_items]
        delete_window = tk.Toplevel(self.root)
        delete_window.title("Delete Loot Item")

        ttk.Label(delete_window, text="Select Item to Delete:").pack(pady=5)
        item_combo = ttk.Combobox(delete_window, values=item_names)
        item_combo.pack(pady=5)

        def confirm_delete():
            name = item_combo.get()
            item = next((item for item in self.loot_items if item.name == name), None)
            if item:
                if messagebox.askyesno("Confirm Delete", f"Delete '{name}'?"):
                    self.loot_items.remove(item)
                    self.update_loot_file()
                    messagebox.showinfo("Deleted", f"Item '{name}' deleted.")
                    delete_window.destroy()
            else:
                messagebox.showerror("Error", "Item not found.")

        ttk.Button(delete_window, text="Delete Item", command=confirm_delete).pack(pady=5)

    def update_preset_listbox(self):
        search = self.preset_search_var.get().lower()
        names = [name for name in self.presets.keys() if search in name.lower()]
        self.preset_listbox.delete(0, tk.END)
        for name in names:
            self.preset_listbox.insert(tk.END, name)

    def update_tag_list(self):
        self.all_tags = sorted({tag for item in self.loot_items for tag in item.tags})

    def show_tags(self):
        tags_str = ", ".join(self.all_tags) if self.all_tags else "No tags available"
        messagebox.showinfo("All Tags", tags_str)

    def update_loot_file(self):
        self.update_tag_list()
        with open(_resolve('data/loot_items.json'), 'w') as file:
            json.dump({
                "items": [item.__dict__ for item in self.loot_items],
                "tags": self.all_tags,
            }, file, indent=4)

if __name__ == "__main__":
    root = tk.Tk()
    app = LootGeneratorApp(root)
    root.mainloop()
