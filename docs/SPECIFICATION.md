# Dynamics CRM Weekly Pipeline Change Analysis

## Problem
The company needs a clear, recurring **weekly analysis** of how the **open weighted sales pipeline** in Dynamics CRM changes over time. They want to understand week-by-week movements in open pipeline weighted value and categorize where these changes come from (e.g., deals won, deals lost, weighted value increases, weighted value decreases), with a mandatory split by **Type of Opportunity** (System Integration (CE) vs Managed Services). Current tools or reports do not provide a structured, repeatable view of these pipeline movements, including the underlying opportunity lines, making it hard to explain changes in open pipeline weighted value to stakeholders.

## MVP Scope
A recurring **weekly** report that:
- Shows how **open pipeline weighted value** changed over a **selected week**.
- Uses a consistent definition of “pipeline” as **open opportunities**.
- Treats opportunities moving to **won or lost** as reasons for **pipeline decrease**.
- Breaks down the change into clear categories (e.g., new opportunities, won, lost, weighted value increase, weighted value decrease, removed if applicable).
- Uses the **Weighted Revenue** field on the opportunity as the basis for all value calculations.
- Allows selection of a specific **week** as the reporting period (user selects the week explicitly).
- Always splits all metrics by **Type of Opportunity**, limited in v1 to:
  - System Integration (CE)
  - Managed Services
- For each movement category, lists the individual opportunities contributing to that category, with key fields (customer, title, owner, **final sales stage at the end of the week or closing stage**).

## Target Users
| User | Primary Need |
|------|--------------|
| Sales leadership / management | Understand why open pipeline **weighted value** (Weighted Revenue) has changed week over week, by opportunity type. |
| Sales operations / CRM analysts | Produce consistent, explainable weekly reports on pipeline movements by opportunity type, with the ability to see the specific opportunities behind each movement. |

## Core Features (v1)

### Feature 1: Weekly Pipeline Weighted Value Overview by Opportunity Type
**User Story**: As a sales leader, I want a weekly overview of how total open pipeline **weighted value** has changed, split by opportunity type, so that I can see which part of the business is driving improvements or declines.

**Acceptance Criteria**:
- User can select a specific reporting week (e.g., “Week starting YYYY-MM-DD” or equivalent explicit week selection).
- System shows, for each opportunity type (System Integration (CE), Managed Services):
  - Starting open pipeline **weighted value** (sum of Weighted Revenue on open opportunities) at the beginning of the week.
  - Ending open pipeline **weighted value** at the end of the week.
  - Net change in open pipeline **weighted value**.
- System also shows a total across the two types.
- Report layout is consistent week to week for recurring use.

### Feature 2: Movement Categorization Within Each Opportunity Type
**User Story**: As a sales operations user, I want weekly changes in open pipeline **weighted value** grouped into categories within each opportunity type so that I can explain what drove the net change in open pipeline.

**Acceptance Criteria**:
- For the selected week, the report shows the contribution of each movement category to the total change in open pipeline **weighted value**, separately for each opportunity type (System Integration (CE), Managed Services).
- Movement categories include at least:
  - **New opportunities** added to open pipeline (using their **Weighted Revenue** when they first enter the pipeline during the week).
  - Opportunities moved to **won** (shown as a decrease in open pipeline weighted value, using the Weighted Revenue that was in the open pipeline before closing).
  - Opportunities moved to **lost** (shown as a decrease in open pipeline weighted value).
  - **Weighted value increases** on existing open opportunities (increase in Weighted Revenue compared to the start of the week).
  - **Weighted value decreases** on existing open opportunities (decrease in Weighted Revenue compared to the start of the week).
  - Opportunities removed from pipeline for other reasons (if applicable and identifiable, e.g., status changes that are not standard won/lost).
- The sum of all category movements reconciles to the net change between starting and ending open pipeline **weighted values** for each opportunity type and for the total.
- All calculations use the **Weighted Revenue** field on the opportunity.

### Feature 3: Opportunity-Level Detail per Movement Category
**User Story**: As a sales operations user, I want to see the individual opportunities behind each movement category so that I can review and explain specific deals that drove the weekly changes.

**Acceptance Criteria**:
- For each movement category within each opportunity type, the report lists the individual opportunities contributing to that category.
- For each listed opportunity, the report shows at least:
  - Customer (account name).
  - Opportunity title.
  - Opportunity owner.
  - **Sales stage as it ends up at the end of the week or at closing** (e.g., Closed – Won, Closed – Lost, or current open stage).
  - Relevant **Weighted Revenue** information for the category (e.g., new Weighted Revenue for new opportunities, increase amount, decrease amount, Weighted Revenue removed when won/lost).
- Users can clearly associate each opportunity line with its movement category (new, won, lost, weighted value increase, weighted value decrease, removed).

## Key Workflows

### Weekly Pipeline Weighted Value Change Analysis by Type
1. User selects a specific week as the reporting period.
2. System retrieves opportunity data and status/value changes from Dynamics CRM.
3. System calculates the open pipeline **Weighted Revenue** at the start of the week and at the end of the week, for each opportunity type (System Integration (CE), Managed Services) and in total.
4. System identifies and quantifies movements by category (new, won, lost, Weighted Revenue increase, Weighted Revenue decrease, removed) within each opportunity type.
5. System presents a summarized weekly report showing, per opportunity type and total:
   - Starting open pipeline Weighted Revenue.
   - Ending open pipeline Weighted Revenue.
   - Net change.
   - Breakdown of net change by movement category.
6. Within each movement category, the system lists the individual opportunities with customer, title, owner, final sales stage, and relevant **Weighted Revenue** information.
7. User reviews the report and uses it for weekly business reviews.

## Future Considerations (post-MVP)
- Monthly and multi-week trend views (e.g., last 12 weeks) using the same Weighted Revenue logic.
- Additional segmentation (e.g., region, industry, account owner) layered on top of opportunity type.
- Automated scheduling and distribution of the weekly report.
- Drill-down to full historical change log for individual opportunities.
- A separate monthly view using the same logic but different period aggregation.
- Parallel reporting on **non-weighted** (nominal) values for comparison.
- Inclusion of other opportunity types beyond System Integration (CE) and Managed Services.

---
<details>
<summary>Technical Notes</summary>

### Data Entities
| Entity | Key Attributes |
|--------|----------------|
| Opportunity | Status (open/won/lost), **Weighted Revenue** (weighted sales value), nominal value, probability (if used for weighting), close date, owner, stage, created date, last modified date, timestamps of status and value changes, **Type of Opportunity** (System Integration (CE), Managed Services), customer (account), title |

### Integrations
- Dynamics CRM (data source) – details to be refined.

### Constraints
- Must respect data access permissions defined in CRM.
- Must handle historical changes to opportunity status and **Weighted Revenue**, if available.
- “Pipeline” is defined as opportunities in an **open** status; this definition must be applied consistently.
- Opportunity type classification must be taken from a reliable field in Dynamics CRM (e.g., “Type of Opportunity”) and used consistently in reporting, limited to System Integration (CE) and Managed Services for v1.
- Opportunity-level listings must be filterable by movement category and opportunity type.
- Sales stage shown for each opportunity is the final stage at the end of the week or at closing (for won/lost).
- All calculations are based on the **Weighted Revenue** field for the opportunity.
- Opportunity types other than **System Integration (CE)** and **Managed Services** are out of scope for v1.

</details>