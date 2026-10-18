## SEN381 — SOFTWARE ENGINEERING 381

## CIVICCONNECT — FINAL PROJECT / MILESTONE 4

## FINALISE • CONSOLIDATE • CONCLUDE

Raw mark: 50 | Team final product/evidence: 25 | Individual engineering defence: 25

## FINAL PROJECT PRINCIPLE — FINALISE, CONSOLIDATE AND CONCLUDE

The Final Project is NOT a new development phase. No new major functionality, architecture, testing programme, deployment infrastructure or engineering artefacts are introduced at this milestone. The remaining project period is shared with the completion of M3, with the Final Project following immediately afterwards. The Final Project therefore does not create a separate new development window. Conclude the project by showing what was originally committed to, what was ultimately delivered, what changed, what remains incomplete, and what the accumulated engineering evidence demonstrates about the final product.

## SHARED M3 / FINAL PROJECT CLOSURE PERIOD

The remaining project period is shared with M3 completion and final project closure. Prioritise completion, integration, defect resolution, stabilisation, evidence consolidation and conclusion.

## Central engineering question

How successfully has the team finalised the product it committed to engineering, and what does the completed project evidence show about the outcome of that engineering journey?

## 1. M4 Identity and Progression

| Milestone | Identity | Question |
| --- | --- | --- |
| M1 | DEFINE | What are we committing to engineer? |
| M2 | DECIDE & DESIGN | How will we engineer it? |
| M3 | CONSTRUCT & VERIFY | Can we build and verify it? |
| M4 | FINALISE & CONCLUDE | What did we finally deliver, how well does it satisfy the original commitment, and what can we conclude from the project? |

## Continuity rule

M4 closes the same project. PED v3.0 continues to PED v4.0. The existing RTM, Risk Register, ADR/Decision Register, Technical Debt Register, Defect Register and AI Usage Register are finalised — not recreated.

## 2. What Students Should Do as M3 Closes and the Final Project Is Concluded

| Priority | Finalisation activity |
| --- | --- |
| 1 | Integrate the work already completed into one coherent final product. |
| 2 | Finish incomplete functionality already inside the approved scope where realistically possible. |
| 3 | Resolve critical defects and integration problems identified during M3. |
| 4 | Stabilise the final version rather than introducing unnecessary new features. |
| 5 | Rerun or confirm the existing critical verification evidence after final fixes. |
| 6 | Update the existing RTM and PED to reflect the actual final status. |
| 7 | Conclude the Risk, Defect, Technical Debt, Decision and AI Usage registers. |


| 8 | Prepare a professional final product demonstration and individual engineering defence. |
| --- | --- |

## Not required

M4 does not require a new testing quota, new architecture/design work, new security functionality, a new performance programme, new observability infrastructure, mandatory backup/recovery implementation.Where such evidence already exists from M3 or the team has capacity to finalise it naturally, it may be demonstrated — but it earns no automatic bonus marks.

## 3. Required Final Outputs

| Output | Final expectation |
| --- | --- |
| Final integrated product | One combined, identifiable and stable final version representing the work completed across the project. |
| Finalise existing functionality | Complete or stabilise important work already within approved scope; avoid unnecessary scope expansion. |
| M1 scope → final delivery reflection | Account for what was originally in scope, what was delivered, what changed, and what remains incomplete. |
| Final acceptance demonstration | Demonstrate selected critical requirements against their acceptance criteria using existing/finalised evidence. |
| M3 issue closure | Show the final status of important M3 defects, failed checks, residual risks or technical debt. Not every minor issue must be eliminated. |
| Final RTM | Continue the existing RTM and give critical requirements/acceptance criteria a final implementation, evidence and status. |
| PED v4.0 | Finalise PED v3.0 into PED v4.0 as the closing engineering record. Do not create a new report. |
| Final project reflection | Conclude what worked, what changed, what remains incomplete, significant decision consequences and what would happen next. |
| Final demonstration | Professionally demonstrate the combined end product and supporting engineering evidence. |
| Individual defence | Each student demonstrates understanding of their contribution and the engineering journey from M1 to final product. |
| Quantified scope completion | Calculate the final Scope Completion Percentage from the approved M1 acceptance-criteria baseline; target at least 80%, with critical/Must-have omissions explicitly justified. |
| Final deployment / go-live decision | Deploy to production where feasible; otherwise use credible simulated production, or defend an evidence-based no-go decision for remaining in staging. |

## 4. The Final Integrated Product

## Primary expectation

The strongest evidence in M4 is a coherent final version of the product students have already been building. The assessment is not looking for a new product. It is looking for the best finalised version of the existing CivicConnect product.

- The final version should integrate the teams completed functionality into one coherent product.

- Critical user journeys should be demonstrable without relying on disconnected prototypes or separate team-member versions.

- Where an important feature is incomplete, the team should stabilise what exists and report the limitation rather than hide it.

- New optional features should not be prioritised ahead of incomplete M1 commitments, critical defects or integration problems.


- The team must identify the final version/build/commit being assessed so that the evidence refers to the same final product.

## 5. Mandatory M1 Scope Baseline → Final Delivery Conclusion

## Return to the original commitment

M1 required an explicit baseline of in-scope, out-of-scope and deferred/future scope together with requirements and acceptance criteria. M4 must now close that loop: How far was the defined scope actually implemented?

| M1 scope item / requirement | M1 priority/status | Final status | Acceptance criteria status | Evidence | Conclusion / reason for variance |
| --- | --- | --- | --- | --- | --- |
| Example: requester submits service request | Must / In scope | Implemented | Met | RTM + final test/demo | Delivered as committed. |
| Example: selected reporting capability | Should / In scope | Partially implemented | Partially met | RTM + demo | Core reporting works; export remains incomplete due to approved reprioritisation. |
| Example: future analytics | Deferred/Future | Still deferred | Not applicable to final committed scope | PED scope baseline | Correctly remained outside the committed final scope. |

## Final status categories

Use: IMPLEMENTED | PARTIALLY IMPLEMENTED | NOT IMPLEMENTED | CHANGED/REMOVED THROUGH APPROVED CHANGE. Deferred/future and out-of-scope items must remain distinguishable from failed commitments.

## USE THE SCOPE COMPLETION PERCENTAGE AS A CONTROL, NOT AS THE ONLY JUDGEMENT

Teams must calculate the required Scope Completion Percentage from the approved M1 acceptance-criteria baseline. The percentage prevents minimal delivery, but marks still consider criticality, stakeholder value, approved changes, evidence quality and the consequence of unmet acceptance criteria.

- Account for every significant original M1 in-scope commitment.

- For partially or not implemented items, state the actual reason using project evidence rather than excuses.

- Explain the effect on stakeholder value or acceptance criteria.

- Distinguish controlled scope change from uncontrolled scope loss.

- Do not treat correctly deferred/out-of-scope work as failure simply because it was not implemented.

- Conclude whether the final delivered scope is coherent and useful even where some lower-priority work remains incomplete.

## 6. Final Acceptance and Existing Quality Evidence

## Finalise — do not create a new testing milestone

M3 established the project’s meaningful verification evidence. M4 should confirm that critical evidence still supports the final version after final fixes. Add tests only where a final change, corrected defect or uncovered risk genuinely requires them.

- Select critical requirements/acceptance criteria from the final RTM and demonstrate their final status.

- Rerun or confirm relevant existing automated checks after important final changes.

- Revalidate the most important integration/E2E journeys where finalisation affected them.

- Update defect status for issues fixed or still open.

- If a previously passing acceptance criterion is no longer satisfied, report it rather than hiding the regression.


- No additional test-count target is introduced in M4.

## 7. M3 → Final Project Closure

| M3 item | What M4 should conclude |
| --- | --- |
| Important open defect | Fixed, still open with limitation, or no longer relevant — with evidence. |
| Failed/conditional quality check | Resolved and rerun where feasible, or carried transparently as a known limitation. |
| Residual risk | Current final status and whether mitigation changed. |
| Technical debt | Repaid where practical or retained with consequence stated. |
| Incomplete integration | Finalised where possible or clearly identified as incomplete. |
| Release/staging evidence | Use existing evidence where still valid; update only where final changes materially affect it. |

## Closure does not mean perfection

Students are not required to eliminate every minor defect or debt item in two weeks. They are required to know the final status of important issues and conclude the project honestly and professionally.

## 8. PED v4.0 — Finalise the Existing Project Engineering Document

## PED v4.0 is project closure, not a new assignment

Take PED v3.0 and bring it to its final state. Preserve the project history. Update only what is needed to represent the final product and project conclusion.

| Finalise in PED v4.0 | What to close/update |
| --- | --- |
| Final product/version | Identify the final assessed build/commit and overall product status. |
| Final scope | Add the M1 scope → final delivery conclusion. |
| Final requirements/acceptance status | Reflect the final RTM and important acceptance outcomes. |
| M3 findings | Update important defects, risks and technical debt to final status. |
| Significant changes/decisions | Record final status/consequence; do not rewrite earlier history. |
| Known limitations/future work | State what remains incomplete or deliberately deferred. |
| Project reflection | Brief evidence-based conclusion on what the project achieved. |
| Evidence links | Point to authentic repository, CI/test and controlled artefact evidence rather than duplicating it. |

## 9. Final RTM — Close the Traceability Chain

## Continue the existing RTM

The RTM remains the project’s traceability backbone. Do not create a fresh “final RTM”. Update the existing one to show the final state of critical requirements and acceptance criteria.

## Final trace

Stakeholder Need → Requirement → Acceptance Criteria → Engineering Decision/Design → Final Implementation → Final Test/Evidence → Defect/Status → Final Outcome.

- Critical M1 requirements should have a clear final status.

- Acceptance criteria should show whether they are met, partially met or unmet.

- Implementation and evidence references should point to the final assessed version.


- Open defects or limitations affecting an acceptance criterion should remain visible.

- The RTM should support the final scope reflection and individual defence.

## 10. Final Project Reflection and Conclusion

## Reflection is evidence-based closure

This is not a long reflective essay. The team should use its accumulated project evidence to reach concise conclusions about the outcome of the engineering work.

| Reflection area | Question to conclude |
| --- | --- |
| Original problem/value | Does the final product meaningfully address the problem and critical stakeholder needs identified in M1? |
| Scope | How far was the approved M1 scope implemented? What changed and why? |
| Quality | What evidence gives confidence in the final product, and what remains uncertain? |
| Risk/defects/debt | Which important issues were resolved and which remain? |
| Engineering decisions | Which 2–3 earlier decisions had the clearest positive or negative consequences? |
| Team/project execution | What most affected the ability to deliver the original commitments? |
| Future | If the project continued, what should be completed or improved next? |

## 11. Final Deployment & Go-Live Evidence

- Existing M3 staging/deployment evidence may be reused where it remains valid.

- At the planned go-live point, the team must declare and defend its final deployment state: production, simulated production, or staging under a justified no-go decision.

- Paid cloud services, containers, multiple servers or advanced deployment tooling are not proxies for Software Engineering quality.

- M4 does not require unnecessary new infrastructure. However, the team must demonstrate the deployment evidence appropriate to its final state and defend the release decision. A team that remains in staging because credible security, privacy, integrity, reliability or operational concerns make release unsafe may earn strong marks when the no-go decision is evidence-based and professionally justified.

## 12. Final Demonstration

| Stage | What the team should show | Guide |
| --- | --- | --- |
| 1. Final product | Identify the final version and demonstrate the combined critical user journey(s). | ~6 min |
| 2. M1 scope → final delivery | Show what was committed, what was implemented/partial/not implemented/changed, and the consequence. | ~6 min |
| 3. M3 → final closure | Show the most important fixes, integration/stabilisation work and remaining limitations. | ~4 min |
| 4. Final evidence | Use the RTM to demonstrate selected acceptance criteria and final evidence. | ~4 min |
| 5. Project conclusion | Conclude what the project achieved, significant decision consequences and what remains. | ~4 min |
| 6. Individual defence | Each student answers assessor questions using authentic project evidence. | Assessed separately |


## Demonstration rule

Do not attempt to show every screen, every test or every document. Demonstrate the final product and use selected evidence to tell the engineering story from original commitment to final outcome.

## 13. Team Assessment — 25 Marks

| Criterion | Marks | Strong evidence | Partial evidence | Weak / missing evidence |
| --- | --- | --- | --- | --- |
| A. Complete, integrated & stable final product | 6 | One coherent final version; critical journeys work; completion/integration/stability prioritised over unnecessary additions. | Product substantially combined but important integration/stability gaps remain. | Disconnected work, major instability or focus on new features while core committed work remains unresolved. |
| B. Quantified M1 scope -> final delivery & acceptance conclusion | 6 | Scope Completion Percentage is correctly calculated; target of at least 80% is met or variance is transparently accounted for; critical/Must-have status, acceptance evidence and controlled changes are clear. | Most scope is accounted for but percentage, acceptance/change evidence or critical- item reasoning is incomplete. | Original baseline is ignored, completion is not quantified, or difficult commitments are silently dropped. |
| C. Final deployment & go-live / no-go decision | 4 | Production or credible simulated production is evidenced; OR a staging no-go decision is strongly justified by credible security/privacy/integrity/reliability/operational evidence. Decision protects stakeholder value rather than merely chasing the deadline. | Deployment state is shown but production- readiness evidence or no-go reasoning is incomplete. | No clear final deployment state; team remains in staging without defensible evidence, or deploys despite known material risks. |
| D. Finalisation of important M3 issues | 3 | Important defects/integration issues/risks/debt have clear final status and were resolved or deliberately carried with evidence. | Some closure shown but important status/evidence gaps remain. | M3 issues ignored, hidden or simply marked complete without evidence. |
| E. Final RTM, PED v4 & existing artefact closure | 3 | Existing artefacts visibly continue and accurately reflect final product, scope, deployment decision and unresolved items without unnecessary recreation. | Artefacts updated but some final trace/status gaps remain. | New/disconnected documents, weak traceability or artefacts not concluded. |
| F. Professional final demonstration & project conclusion | 3 | Clear combined-product demonstration and concise evidence-based conclusion on outcome, limitations, deployment decision and significant decision consequences. | Demonstration works but conclusion/evidence selection is uneven. | Screen tour with little engineering conclusion or inability to explain final state. |

## 14. Individual Engineering Defence — 25 Marks

| Criterion | Marks | What each student must demonstrate |
| --- | --- | --- |
| A. Contribution & implemented functionality | 5 | Explain their contribution and demonstrate understanding of how it fits the combined final product. |
| B. M1 requirement → final evidence trace | 5 | Trace an original stakeholder need/requirement |


## SEN381 SOFTWARE ENGINEERING 381 | CIVICCONNECT | FINAL PROJECT

|   |   | through acceptance criteria to final implementation/evidence/status. |
| --- | --- | --- |
| C. Engineering decisions & consequences | 5 | Defend important architecture/design/technology decisions and explain consequences that became visible during implementation/finalisation. |
| D. Quality, defects, risks & limitations | 5 | Interpret relevant evidence and explain what remains incomplete, risky or technically indebted without overstating quality. |
| E. Project outcome, learning & future improvement | 5 | Reach a professional conclusion about the project outcome, explain learning from the engineering journey, AI accountability where applicable, and identify sensible next work. |

## Individuality rule

Students in the same team may receive different individual marks. Every student must understand the combined product and lifecycle evidence, not only the code or artefact they personally created.

## 15. Recommended Two-Week Work Plan

| Period | Priority | Expected result |
| --- | --- | --- |
| As M3 approaches submission | Freeze unnecessary scope expansion; review M1 commitments and M3 open issues. | Agreed completion priorities. |
| M3 completion period | Integrate, finish critical in-scope work, fix important defects and stabilise. | Coherent M3 release candidate. |
| Immediately after M3 | Resolve only priority final issues, calculate scope completion and confirm final acceptance evidence. | Final candidate and quantified scope status. |
| Before final submission | Make and evidence the go-live decision: production, simulated production, or justified staging no-go. | Defensible final deployment state. |
| Final closure | Finalise RTM, PED v4 and registers so they match the frozen final release. | Project records match actual final state. |
| Presentation / defence | Demonstrate the frozen final version and defend scope, evidence, deployment decision, limitations and conclusions. | Concluded project and individual defence. |

## Final Scope Completion Requirement

The Final Project must demonstrate substantial completion of the scope that the team approved in M1. The purpose of quantifying completion is to prevent teams from presenting only a minimal subset of the original commitment while still allowing legitimate controlled change.

Target: teams should demonstrate at least 80% completion of the approved M1 in-scope acceptance criteria, with all critical / Must-have requirements expected to be implemented unless an approved change, unresolved high-risk defect, security concern or other defensible engineering constraint justifies a different final status.

## Scope Completion Calculation

For transparency, calculate a Scope Completion Percentage from the M1 baseline. Each acceptance criterion is given a final status: Met = 1.0, Partially Met = 0.5, Not Met = 0.0. Approved removals or changes are treated according to the formally changed baseline and must have a traceable change reference.


Scope Completion % = (sum of final acceptance-criterion completion values ÷ number of applicable approved acceptance criteria) × 100.

The percentage is evidence, not the whole judgement. Students may not inflate completion by counting trivial items while leaving critical stakeholder requirements incomplete. The assessor must consider priority, stakeholder value, approved changes and the significance of unmet acceptance criteria.

| Final scope result | Interpretation for M4 |
| --- | --- |
| 90–100% | Very high completion of the approved scope. Strong evidence is still required; percentage alone does not guarantee high marks. |
| 80–89% | Expected substantial-completion range, provided critical / Must-have requirements and acceptance evidence are credible. |
| 60–79% | Material scope remains incomplete. Team must clearly account for omissions, priorities, approved changes and consequences. |
| Below 60% | Major portion of approved scope remains undelivered. Strong justification may explain the outcome but does not convert missing delivery into completed scope. |

Safety and professional judgement override deadline pressure: a team must not be encouraged to claim a requirement is complete, suppress a known defect, weaken a control or release unsafe functionality merely to improve its completion percentage.

## Final Deployment & Go-Live Decision

Because the project has reached its planned go-live point, the team must make and defend a final release decision. Where reasonably feasible, the final application should be deployed to a production environment. Where real production deployment is not feasible, a credible simulated production environment is acceptable. Remaining in staging is also acceptable when the team can demonstrate that proceeding to production would create an unjustified security, privacy, data-integrity, reliability, operational or other material risk.

| Final deployment state | M4 expectation |
| --- | --- |
| Production | Identify the final release, production configuration, controlled secrets, deployment evidence and basic post- deployment verification. |
| Simulated production | Demonstrate a production-like environment separated from development, with release identification, production-like configuration, controlled secrets and deployment verification. |
| Remained in staging | Defend the no-go decision using specific evidence. Identify the blocking concern, affected requirement/risk, likely consequence of releasing, and what must be resolved before go-live. |

A team must not lose marks simply because it refused to race into production to meet a deadline. A well-evidenced no-go decision based on a credible security, privacy, integrity, reliability or operational concern is sound software-engineering judgement and can earn strong marks for release readiness and professional decision-making. Conversely, remaining in staging because the team did not plan, integrate or complete the work is not automatically a valid engineering precaution.

The assessor should therefore judge the quality of the release decision, not reward production deployment merely for its own sake. The question is: given the evidence available at the deadline, did the team make a responsible and defensible go-live / no- go decision?


## Submission Requirements & Submission Procedure

M4 is the finalisation and conclusion of the existing CivicConnect project. Students are not expected to create a new Final Project Report or duplicate evidence that already exists in the controlled project environment. PED v4.0 Final is the concluding version of the Project Engineering Document and acts as the primary final evidence index.

The final assessed product must correspond to the exact repository commit/tag/version identified at submission. Evidence in PED v4.0, the final RTM and the project registers must describe that same final baseline.

| Submission item | What is expected | How it is submitted / evidenced |
| --- | --- | --- |
| PED v4.0 Final | The existing PED v3.0 finalised and concluded. It should summarise the final product, final scope status, project outcome, important limitations and evidence locations. | Submit as the primary final project document, preferably PDF if required by Moodle. |
| Final GitHub repository | The same controlled team repository used throughout the project, with authentic history preserved. | Submit the repository URL. Do not create a separate source-code ZIP unless specifically instructed. |
| Final release baseline | The exact final commit hash, tag or release/version that represents the product being assessed. | State it clearly in PED v4.0 and in the Moodle submission fields where available. |
| Final RTM | The existing RTM brought to final status, showing the final relationship between requirements, acceptance criteria, implementation, tests/evidence and final status. | Include in PED v4.0 or maintain as a controlled linked artefact referenced from PED v4.0. |
| Final project registers | Existing Risk, Defect, Technical Debt, Decision/ADR/Change and AI Usage records brought to an appropriate final status. | Do not recreate them. Link/reference the controlled artefacts from PED v4.0. |
| M1 Scope -> Final Delivery reflection | Every original M1 in-scope commitment must have a final status and evidence. Deferred, future and out-of-scope items must remain distinguishable from failed commitments. | Include the final scope reflection table in PED v4.0. |
| Final combined product | The most complete and stable version of the product developed through M1- M3. No new major feature set is required for M4. | Demonstrate live using the identified final release baseline. |
| Final presentation & individual defence The team concludes the engineering | journey and each student demonstrates understanding of the final product, evidence, decisions, limitations and contribution. | Conducted live. Presentation slides may support the defence but do not replace the product/evidence demonstration. |

## Minimum Moodle / Formal Submission

- PED v4.0 Final.

- Final GitHub repository URL.

- Exact final commit/tag/release identifier.

- Any access information specifically required to view the final product or controlled evidence.

All other evidence should remain in the existing controlled project artefacts or repository and be referenced from PED v4.0. Students should not submit multiple disconnected copies of the same evidence.

Submission baseline rule: the final version assessed is the identified repository commit/tag/release. Changes made after the submission deadline are not part of the submitted final project unless explicitly authorised.


## Final Submission Information Block

- Team name / number:

- Team members:

- GitHub repository URL:

- Final commit / tag / release:

- PED version: v4.0 Final

- Final RTM location:

- Final product access / demonstration information:

- Date submitted:

## Expected Assessor Questions — Final Project Engineering Defence

The Final Project defence is intended to establish that students understand and can conclude the engineering work they have already performed. It is not intended to introduce new implementation requirements. The assessor may adapt questions to the actual final product and may ask students to locate supporting evidence in PED v4.0, the RTM, GitHub history, tests or project registers.

These are representative question types, not a fixed question bank. Students should prepare to reason from their own final product and project evidence rather than memorise model answers.

## 1. Final Product & Overall Outcome

- Show the final combined product. Which critical user journeys are complete and stable in this final version?

- What is the exact commit/tag/release being demonstrated, and how do we know it is the same version described in PED v4.0?

- What changed between the M3 release candidate and this final version?

- What can you now conclude about the project that you could not yet conclude at M3?

## 2. M1 Scope -> Final Delivery

- Choose one important item that was placed in scope in M1. Was it fully implemented, partially implemented, not implemented, or changed through approved change? Show the evidence.

- Which M1 commitment was most difficult to complete, and what affected its final status?

- Show one item that was deliberately deferred or out of scope. Why should that not automatically be treated as project failure?

- Did any scope change occur after M1? Was it controlled, and what consequence did the change have for the project?

- Looking at the original M1 scope as a whole, what can the team conclude about how far the agreed scope was delivered?

## 3. Requirements, Acceptance Criteria & Final Evidence

- Trace one important stakeholder need through requirement, acceptance criterion, final implementation and final evidence.

- Which acceptance criterion remains only partially satisfied, if any, and what is the consequence?

- Why does demonstrating a working screen not by itself prove that a requirement has been accepted?

- Which final requirement has the strongest evidence and which has the weakest? Explain using the RTM.

## 4. M3 Issues -> Final Status

- Identify an important defect, failed check, integration issue or residual risk from M3. What is its final status?

- As the project approached M3 and final submission, what did the team deliberately prioritise for completion, and why?

- Which M3 issue remains unresolved? Why was it not completed, and what consequence does it have for the final product?

- Show evidence that an important M3 fix was actually reverified rather than simply marked as complete.


## 5. Engineering Decisions & Consequences

- Choose one significant decision made earlier in the project. What consequence can you now observe in the final product?

- Which earlier decision helped the team most during finalisation?

- Which earlier decision created difficulty, technical debt or additional work later?

- With the benefit of the completed project, would you retain that decision or change it? Explain from evidence rather than hindsight alone.

## 6. Quality, Limitations, Risk & Technical Debt

- What evidence gives you confidence in the final product?

- What important limitation should the assessor or a future engineering team know about?

- What technical debt remains, and what future maintenance consequence could it create?

- If this product had to be used by real users tomorrow, what would concern you most and why?

- What does the team know is working, and what remains uncertain?

## 7. Finalisation & Project Closure

- Between the M3 release candidate and final submission, what did the team finalise, stabilise or conclude rather than newly develop?

- How did you decide what to finish and what not to expand?

- Show how PED v3.0 became PED v4.0 without losing the engineering history of the project.

- Which project artefacts are now closed, and which items deliberately remain open for future work?

- What evidence shows that the project has been concluded rather than simply stopped?

## 8. Individual Contribution & Engineering Understanding

- What was your individual contribution to the final product and finalisation work? Show evidence.

- Trace one part of the product you worked on from requirement to final evidence.

- Explain one engineering decision made by the team that affects a component you did not personally implement.

- What problem did you personally help resolve during the project, and what did you learn from the outcome?

## 9. Unseen Change / Failure Reasoning

- If a stakeholder requested a new feature today, what existing artefacts would you examine before agreeing to the change?

- If one critical final test failed during this demonstration, what would you investigate first?

- If another engineering team inherited this system tomorrow, what information would they need before changing it safely?

- If one unresolved limitation suddenly became a mandatory requirement, which parts of the existing system could be affected?

## 10. Final Project Conclusion

- What is the strongest evidence that the project delivered stakeholder value?

- Where did the final outcome differ from the team's original expectations in M1?

- What should a future team retain from this project, and what should they improve?

- In one evidence-based conclusion, what did this project ultimately achieve and what remains unfinished?

## 11. Quantified Scope Completion & Go-Live Decision

- What is your final Scope Completion Percentage, and show how you calculated it from the M1 acceptance-criteria baseline.

- Which critical or Must-have requirement remains incomplete, if any, and why?

- Did any approved scope change alter the denominator or status of the original M1 commitment? Show the change evidence.

- What is the team's final deployment state: production, simulated production, or staging?

- If you went to production, what evidence supports the decision that the release was ready?


- If you remained in staging, what specific risk or unresolved concern justified a no-go decision at the planned go-live deadline?

- What would have to change before the team could responsibly approve production release?

## How Students Should Prepare

- Prepare to demonstrate the final product and navigate the actual project evidence; do not memorise definitions or scripted answers.

- Every student should be able to explain at least one M1 scope commitment from original requirement through final delivery status and evidence.

- Know the final status of important M3 defects, risks and technical debt, including anything deliberately left unresolved.

- Be prepared for follow-up prompts such as: Why? Show me. Where is the evidence? What changed? What is the consequence? What remains incomplete?

- Students are not expected to defend functionality or engineering work that M4 did not require them to add.

## Assessor Questioning Principle

The assessor may ask different questions of different students. Questioning should sample the individual assessment dimensions: contribution and implemented functionality; M1 requirement/acceptance-to-final-evidence trace; engineering decisions and consequences; quality, defects, risks and limitations; and project outcome, learning and future improvement. Follow-up questions may be used where an answer is rehearsed, unsupported, unclear or inconsistent with the final project evidence.

## 16. Final Submission Checklist

- ☐ One final integrated product/version is clearly identified.

- ☐ The team prioritised finalisation and stability rather than unnecessary new features.

- ☐ Every significant M1 in-scope commitment has a final status.

- ☐ Partially/not implemented scope is explained honestly with its acceptance/stakeholder consequence.

- ☐ Deferred/future and out-of-scope work is not incorrectly treated as failed scope.

- ☐ Approved scope changes remain traceable.

- ☐ Critical final user journeys are demonstrable.

- ☐ Important M3 defects/integration issues/risks/debt have a final status.

- ☐ Existing critical verification evidence still supports the final version or has been updated where necessary.

- ☐ No new M4 test-count target has been created.

- ☐ The existing RTM has been updated to final status; it was not recreated.

- ☐ PED v3.0 has been finalised as PED v4.0; it was not replaced with a new report.

- ☐ Risk, Defect, Decision/ADR, Technical Debt and AI Usage records are concluded appropriately.

- ☐ Known limitations and future/deferred work are clearly stated.

- ☐ The team can conclude how far the M1 scope was implemented and what the project achieved.

- ☐ The final presentation demonstrates the combined product rather than every screen/document.

- ☐ Every student is prepared to defend their contribution, a requirement-to-evidence trace, decisions, quality/risks and project outcome.

## 17. Final Engineering Principle

## M4 is closure

Do not start another project inside the Final Project. Finalise what you committed to. Consolidate the evidence you have


built. Conclude what the project actually achieved.

## Final defence pattern

M1 commitment → Requirement → Acceptance Criteria → Engineering decisions → Final implementation → Evidence → Final status → Project conclusion.

- Final Scope Completion Percentage calculated from the approved M1 acceptance-criteria baseline.

- At least 80% scope completion targeted; every critical / Must-have omission explicitly justified and evidenced.

- Final deployment state explicitly declared: production, simulated production, or staging.

- Go-live / no-go decision supported by evidence rather than deadline pressure.

- Where the team remains in staging, the blocking security, privacy, integrity, reliability or operational concern and required remediation are documented.
