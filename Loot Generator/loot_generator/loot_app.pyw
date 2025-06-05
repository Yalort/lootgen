import tkinter as tk
from tkinter import ttk, messagebox, scrolledtext, simpledialog
from utils import *

class LootGeneratorApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Loot Generator")
        self.loot_items = load_loot_items()
        self.presets = load_presets()

        self.setup_ui()

    def setup_ui(self):
        # Inputs
        frame = ttk.Frame(self.root, padding="10")
        frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(frame, text="Loot Points:").grid(row=0, column=0, sticky=tk.W)
        self.loot_points_entry = ttk.Entry(frame)
        self.loot_points_entry.grid(row=0, column=1, sticky=tk.EW)

        ttk.Label(frame, text="Tags (comma-separated):").grid(row=1, column=0, sticky=tk.W)
        self.tags_entry = ttk.Entry(frame)
        self.tags_entry.grid(row=1, column=1, sticky=tk.EW)

        ttk.Button(frame, text="Generate Loot", command=self.generate_loot_items).grid(row=2, column=0, columnspan=2, pady=5)

        # Preset Controls
        ttk.Label(frame, text="Preset:").grid(row=3, column=0, sticky=tk.W)
        self.preset_combo = ttk.Combobox(frame, values=list(self.presets.keys()))
        self.preset_combo.grid(row=3, column=1, sticky=tk.EW)

        ttk.Button(frame, text="Load Preset", command=self.load_preset).grid(row=4, column=0, columnspan=2, pady=5)
        ttk.Button(frame, text="Save Preset", command=self.save_preset).grid(row=5, column=0, columnspan=2)
        ttk.Button(frame, text="Delete Preset", command=self.delete_preset).grid(row=6, column=0, columnspan=2)

        # Loot Output
        ttk.Label(frame, text="Generated Loot:").grid(row=7, column=0, sticky=tk.W)
        self.output_area = scrolledtext.ScrolledText(frame, height=10)
        self.output_area.grid(row=8, column=0, columnspan=2, sticky=tk.NSEW, pady=5)

        frame.columnconfigure(1, weight=1)
        frame.rowconfigure(8, weight=1)

    def generate_loot_items(self):
        points = int(self.loot_points_entry.get())
        tags = [tag.strip() for tag in self.tags_entry.get().split(',')] if self.tags_entry.get() else None

        loot = generate_loot(self.loot_items, points, tags)

        self.output_area.delete('1.0', tk.END)
        for item in loot:
            self.output_area.insert(tk.END, f"{item.name} ({item.rarity}) - {item.description} [{item.point_value} points]\n")

    def load_preset(self):
        preset_name = self.preset_combo.get()
        preset = self.presets.get(preset_name)

        if preset:
            self.loot_points_entry.delete(0, tk.END)
            self.loot_points_entry.insert(0, str(preset['loot_points']))
            self.tags_entry.delete(0, tk.END)
            self.tags_entry.insert(0, ', '.join(preset['tags']))
        else:
            messagebox.showerror("Error", "Preset not found.")

    def save_preset(self):
        preset_name = simpledialog.askstring("Save Preset", "Preset Name:")
        if preset_name:
            points = int(self.loot_points_entry.get())
            tags = [tag.strip() for tag in self.tags_entry.get().split(',')]

            self.presets[preset_name] = {"loot_points": points, "tags": tags}
            save_presets(self.presets)
            self.preset_combo['values'] = list(self.presets.keys())
            messagebox.showinfo("Saved", f"Preset '{preset_name}' saved successfully!")

    def delete_preset(self):
        preset_name = self.preset_combo.get()
        if preset_name in self.presets:
            if messagebox.askyesno("Delete", f"Are you sure you want to delete '{preset_name}'?"):
                del self.presets[preset_name]
                save_presets(self.presets)
                self.preset_combo['values'] = list(self.presets.keys())
                messagebox.showinfo("Deleted", f"Preset '{preset_name}' deleted.")
        else:
            messagebox.showerror("Error", "Preset not found.")

if __name__ == "__main__":
    root = tk.Tk()
    app = LootGeneratorApp(root)
    root.mainloop()
