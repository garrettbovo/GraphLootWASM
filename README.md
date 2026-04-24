# GraphLoot Engine - Blazor WASM

A free, browser-based visualization of your C++ Graph Loot Engine. This is a **C# port** of your C++ engine compiled to WebAssembly, running entirely in the browser with zero server cost.

**Live at:** (your GitHub Pages URL once deployed)

## What This Is

- **Pure client-side:** Runs 100% in your browser. No server. No cost.
- **C# port of your engine:** Dijkstra's algorithm, A*, loot simulation, all implemented in C# matching your C++ logic.
- **Interactive visualization:** Click nodes on the graph to set start/end points, compute paths, run large-scale simulations.
- **Deploy to GitHub Pages:** One-time setup, then push to GitHub and it's live.

## Setup (Local)

### Prerequisites
- .NET 10 SDK or later
- Your CSV files from GraphLoot-Engine:
  - `DefaultMap.csv`
  - `Nodes.csv`
  - `Items.csv`

### Install & Run

```bash
# Clone this repo
git clone https://github.com/yourusername/GraphLoot-Engine-WASM.git
cd GraphLootWASM

# Copy your CSV files into wwwroot/data/
cp ../GraphLoot-Engine/DefaultMap.csv wwwroot/data/
cp ../GraphLoot-Engine/Nodes.csv wwwroot/data/
cp ../GraphLoot-Engine/Items.csv wwwroot/data/

# Run locally
dotnet watch run
```

Open `https://localhost:7000` in your browser (exact port varies; check the console output).

## Deploy to GitHub Pages (Free)

This app is designed to work perfectly with **GitHub Pages**, which hosts static files for free.

### One-Time Setup

1. **Enable GitHub Pages** on your repo:
   - Go to Settings → Pages
   - Source: Deploy from a branch
   - Branch: `main` (or wherever you'll push)

2. **Create a GitHub Actions workflow** to build and deploy:
   Create `.github/workflows/deploy.yml`:

   ```yaml
   name: Deploy to GitHub Pages

   on:
     push:
       branches: [ main ]

   jobs:
     deploy:
       runs-on: ubuntu-latest
       steps:
       - uses: actions/checkout@v3

       - name: Setup .NET
         uses: actions/setup-dotnet@v3
         with:
           dotnet-version: '10.0.x'

       - name: Publish
         run: dotnet publish -c Release -o dist

       - name: Deploy
         uses: peaceiris/actions-gh-pages@v3
         with:
           github_token: ${{ secrets.GITHUB_TOKEN }}
           publish_dir: ./dist/wwwroot
   ```

3. **Commit and push** the workflow file:
   ```bash
   git add .github/workflows/deploy.yml
   git commit -m "Add GitHub Pages deployment workflow"
   git push
   ```

4. **Wait for the action to finish**, then go to `https://yourusername.github.io/GraphLoot-Engine-WASM/` (or your custom domain if set up).

### To Update Later

Just push changes:
```bash
git add .
git commit -m "Update simulation logic"
git push
```

GitHub Actions will automatically rebuild and redeploy.

## How It Works

**Engine structure:**
- `Engine/Graph.cs` — Dijkstra + A* pathfinding (C# port)
- `Engine/GameEngine.cs` — Simulation loop and CSV loading
- `Engine/ItemDatabase.cs` — Loot generation and probability
- `Components/Pages/Home.razor` — Interactive UI with SVG graph rendering

**Data flow:**
1. Load `Nodes.csv`, `DefaultMap.csv`, `Items.csv` from `wwwroot/data/`
2. Build the graph in-memory
3. When you click nodes: compute shortest path using Dijkstra or A*
4. When you run simulation: aggregate loot distributions across 1000+ runs

**No server calls.** Everything is local to your browser.

## Customization

### Change CSV Locations
Edit the paths in `Components/Pages/Home.razor` (currently loads from `wwwroot/data/`):
```csharp
var nodesCsv = await Http.GetStringAsync("data/Nodes.csv");
```

### Adjust Simulation Logic
Edit `Engine/GameEngine.cs` to change how loot drops are generated or how paths are scored.

### Styling
Modify `wwwroot/app.css` — it uses CSS variables (`:root { --accent: #58a6ff; }` etc) so you can rebrand easily.

## Known Limitations

- **C# port, not C++**: The engine logic is a faithful port of your C++ implementation, but it's not the exact same code. Both produce equivalent results.
- **Browser performance**: Simulations run single-threaded in the browser. 100,000 runs takes ~5 seconds; 1,000,000 takes ~50 seconds. For larger sims, run local or use Web Workers (advanced).
- **CSV parsing is simple**: Assumes no quoted fields or escaped commas. If your CSVs have complex formatting, adjust `ItemDatabase.LoadFromCSV()`.

## File Tree

```
GraphLootWASM/
├── GraphLootWASM.csproj
├── Program.cs
├── Engine/
│   ├── Models.cs          # Data structures
│   ├── Graph.cs           # Dijkstra, A* algorithms
│   ├── GameEngine.cs      # Simulation loop
│   └── ItemDatabase.cs    # Loot system
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── _Imports.razor
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Pages/
│       ├── Home.razor     # Main UI (graph + controls)
│       └── BarChart.razor # Distribution charts
├── wwwroot/
│   ├── index.html
│   ├── app.css
│   └── data/              # DROP YOUR CSVs HERE
│       ├── Nodes.csv
│       ├── DefaultMap.csv
│       └── Items.csv
└── .github/workflows/
    └── deploy.yml         # GitHub Actions deployment
```

## Troubleshooting

**"Failed to load data: 404"**
- The CSV files aren't in `wwwroot/data/`. Copy your actual CSVs there and rebuild.

**Simulation is slow**
- Browser JavaScript is not as fast as native C++. This is expected. Consider running local or breaking sims into smaller batches.

**Graph won't display**
- Check browser console (F12) for errors. Ensure Nodes.csv has `name,x,y` format and DefaultMap.csv has `from,to,weight`.

**Deploy failed on GitHub**
- Check the Actions tab for error logs. Common issue: .gitignore excludes required files. Ensure `wwwroot/data/*.csv` are committed.

## Attribution

This project is a **Blazor WASM port** of your **C++ GraphLoot Engine**. The core algorithms (Dijkstra, A*, loot probability) are implemented in C#. The visualization and interactivity are added by Blazor.

For the original C++ engine with command-line interface, see: [garrettbovo/GraphLoot-Engine](https://github.com/garrettbovo/GraphLoot-Engine)

## License

Same as your GraphLoot-Engine repository.
