# Publisher Guard — Local Enhancement Pack 0.2

## Product goal

The next upgrade makes Publisher Guard more useful for a creator who repeatedly prepares digital delivery folders, without turning it into cloud software, a marketplace uploader, or a complicated enterprise DAM tool. The creator should see what is included, what has been held back, what changed since another package, and what the final delivery contains.

## Chosen additions

| Addition | User value | Local-first contract |
|---|---|---|
| **Package presets** | Starts a review with a recognisable intent instead of technical rules. | Profiles are local configuration only; the first release ships with Standard Asset Pack as the selected default. |
| **Quick-check summary** | Shows included, excluded, and action-needed counts before the user opens technical details. | Counts derive only from the in-memory local inventory and preflight result. |
| **Ignore rules** | Lets the creator exclude known local artifacts without repeating the same decision. | Rules are stored below `%LocalAppData%\CyberBank\Publisher Guard`, never in the source package and never synced. |
| **Inventory comparison** | Shows added, removed, and changed relative files between two local package inventories. | Comparison uses relative paths and SHA-256 only; it reads metadata and never executes either folder. |
| **Readable delivery report** | Gives a creator or client a simple proof of what was delivered. | `delivery-report.md` sits beside JSON manifest/audit and contains no absolute source root or file bytes. |

## Data contracts

`PackagePreset` is an immutable identifier, name, and short local description. `StandardAssetPack` remains the default and preserves existing README behavior. Future presets must be explicit domain contracts, not hidden string switches.

`LocalIgnoreRule` records a relative path or simple file-name pattern, human reason, and UTC creation time. An ignore rule can only transition an inventory entry from Included to Excluded. It cannot re-include executable or system artifact entries, cannot escape a selected source root, and must never alter the original source file.

`InventoryComparison` compares two in-memory inventory snapshots by relative path, then uses SHA-256 to distinguish unchanged from changed files. It returns three deterministic sets: Added, Removed, and Changed.

`DeliveryReport` is generated only after the atomic output promotion has succeeded. It lists delivery status, profile, included/excluded totals, local verification instructions, and review decisions. It does not claim legal compliance, marketplace acceptance, or remote delivery.

## Implementation order

1. Implement and test `PackagePreset` plus quick-check summary projection.
2. Implement and test persisted local ignore rules, then apply them before preflight.
3. Implement and test SHA-256 inventory comparison.
4. Implement and test readable `delivery-report.md` in atomic output.
5. Add concise Workstation panels/actions only after each core contract is green.

## Safety challenges answered

| Challenge | Decision |
|---|---|
| Could ignore rules hide dangerous files? | No. Rules cannot override automatic executable/system exclusions and remain visible in delivery evidence. |
| Could comparison read or run package content? | No. It compares relative metadata and SHA-256 inventory values only. |
| Could report expose sensitive local paths? | No. Absolute source/output roots are forbidden from report, manifest, and audit assertions. |
| Could presets create accidental legal claims? | No. Presets define review expectations only and use plain language such as “needs attention,” never “approved” or “compliant.” |

## External references

1. National Centers for Environmental Information, [Standard Practices for Ensuring Data Integrity](https://ioos.github.io/ncei-archiving-cookbook/practices.html). The guidance supports the existing choice of per-file SHA-256 values plus a manifest that names package files and their checksums.
2. Filecamp, [Digital Asset Management Workflow](https://filecamp.com/blog/digital-asset-management-workflow/). The workflow discussion supports surfacing plain review states and useful metadata without turning Publisher Guard into a cloud DAM product.
