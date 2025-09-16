# Documentation Consolidation Plan for TrainArena

## Current State
We have multiple README files with overlapping content and inconsistent naming:

1. **Main Documentation**:
   - `README.md` (project root) - Comprehensive feature map and quickstart
   - `QUICKSTART.md` (project root) - Duplicate of main README content

2. **Legacy/Redundant Documentation**:
   - `TrainArena/README.md` - Original minimal version 
   - `TrainArena/README_v3.md` - Add-on features (self-play, domain randomization)
   - `TrainArena/README_Extras.md` - TensorBoard dashboard & model hot-reload
   - `TrainArena/README_TrainArena.md` - Brief summary version
   - `TrainArena/TRAINARENA.md` - Simple overview

3. **Planning Documentation**:
   - `TrainArena/PLAN.md` - Development roadmap and progress tracking

## Recommended Consolidation Strategy

### Keep and Improve
1. **`README.md`** (root) - Main project documentation
   - Comprehensive feature overview
   - Installation and setup instructions
   - Complete feature map with usage examples
   - Training commands and workflows

2. **`QUICKSTART.md`** (root) - Fast getting-started guide
   - Streamlined 5-minute setup
   - Essential commands only
   - Link to main README for details

3. **`TrainArena/PLAN.md`** - Development tracking
   - Keep as-is for project management
   - Day-by-day development schedule
   - Technical task breakdown

### Archive or Remove
1. **`TrainArena/README*.md`** files - Archive these legacy files
   - Move useful content into main README.md
   - Remove redundant files to reduce confusion

### Create New Structure
```
/
├── README.md                    # Main documentation (comprehensive)
├── QUICKSTART.md               # Fast setup guide (streamlined)
├── docs/
│   ├── FEATURES.md             # Detailed feature documentation
│   ├── TRAINING.md             # Training guides and tips
│   ├── TROUBLESHOOTING.md      # Common issues and solutions
│   └── API.md                  # Code API reference
└── TrainArena/
    ├── PLAN.md                 # Development plan (keep)
    └── archive/                # Move old READMEs here
        ├── README_legacy.md
        ├── README_v3.md
        └── README_Extras.md
```

## Benefits
- Single source of truth for users
- Clear navigation path: QUICKSTART → README → detailed docs
- Reduced maintenance burden
- Better organization for different user needs
- Preserved historical content in archive

## Implementation Priority
1. ✅ Fix technical issues (pink obstacles, tag errors)
2. 🔄 Currently working: Consolidate documentation
3. ⏳ Create organized docs/ structure
4. ⏳ Update PLAN.md progress tracking