[app]
title = Loot Generator
package.name = lootgen
package.domain = org.example
source.dir = .
source.include_exts = py,png,jpg,kv,json
source.exclude_dirs = tests
requirements = python3,kivy
entrypoint = loot_generator/android_app.py
orientation = portrait

[buildozer]
log_level = 2
warn_on_root = 1
