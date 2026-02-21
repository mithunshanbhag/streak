# APP CHECKLIST

This is my opinionated checklist for building indie-SaaS, micro-SaaS apps. It is not exhaustive, but it covers some important aspects of my app building process. This is a living document and will be updated / tweaked as required.

## Preferred tech stack

My preferred framework for building apps is .NET (currently .NET 10 is the latest version):

- Frontend: Blazor WebAssembly (with MudBlazor controls).
- Backend: Azure Function Apps.
- Database: Azure Cosmos DB (NOSQL API, formerly known as Core SQL API).

I prefer to host my apps and related infra on Azure using serverless/PaaS. This is to keep things relatively simple, low maintenance and low cost.

## Steps

### Step 1 - Requirements

### Step 2 - Mockups

- [ ] Mockups are created for all screens and states of the app.
- [ ] The mockups are in the form of a Figma (`.fig`) file with the following contents:
  - [ ] A page for each screen of the app, including all states (e.g., loading, error, empty states).
  - [ ] A **style guide** page that includes the color palette, typography, and component library used in the app.
  - [ ] An **assets page** that includes all icons, images, and other visual elements used in the app.
  - [ ] A prototype page that demonstrates the user flow through the app, including interactions and animations.

### Step 3 - Implementation

### Step 4 - Testing

### Step 5 - Deployment

### Step 6 - Monitoring

### Step 7 - User Feedback and Improvement

### Step 8 - Maintenance and Iteration

## Cross cutting concerns

- [ ] Accessibility considerations have been addressed, including support for screen readers, keyboard navigation, and color contrast.
- [ ] The app has been tested on multiple devices and screen sizes to ensure responsiveness and usability.
- [ ] Performance optimizations have been implemented, including lazy loading of images and code splitting.
- [ ] The app has been tested for security vulnerabilities, including input validation and protection against common attacks such as XSS and CSRF.
- [ ] The app includes analytics tracking to monitor user behavior and app performance.
- [ ] The app includes error handling and logging to capture and report issues.
- [ ] The app includes a user feedback mechanism to collect user input and suggestions for improvement. 
- [ ] The app includes localization support for multiple languages and regions.
- [ ] The app includes offline support and caching to improve performance and usability in low-connectivity environments.
- [ ] The app includes a comprehensive testing suite, including unit tests, integration tests, and end-to-end tests.
- [ ] The app includes documentation for developers, including setup instructions, architecture overview, and API documentation.
- [ ] The app includes user documentation, including a user guide and FAQ section.
- [ ] The app includes a deployment pipeline for continuous integration and delivery.
- [ ] The app includes monitoring and alerting to detect and respond to issues in production.
- [ ] The app includes a versioning strategy to manage releases and updates.
- [ ] The app includes a backup and recovery plan to protect against data loss.
- [ ] The app includes a maintenance plan to ensure ongoing support and updates.
- [ ] The app includes a roadmap for future development and feature enhancements.
- [ ] The app complies with relevant legal and regulatory requirements, including data privacy and accessibility standards.
- [ ] The app has been reviewed and approved by relevant stakeholders, including product owners, designers, and developers.
- [ ] The app has been tested with real users to gather feedback and identify areas for improvement.
- [ ] The app has been optimized for search engines (SEO) to improve visibility and discoverability.
- [ ] The app includes social media integration to enable sharing and engagement.
- [ ] The app includes marketing and promotional materials to support user acquisition and retention.
- [ ] The app includes a support plan to provide assistance to users and address issues.
- [ ] The app includes a community engagement plan to foster user interaction and feedback.
- [ ] The app includes a monetization strategy to generate revenue and sustain development.
- [ ] The app includes a legal disclaimer and terms of service to protect against liability and ensure compliance.
- [ ] The app includes a privacy policy to inform users about data collection and usage practices.
- [ ] The app includes a cookie policy to inform users about the use of cookies and tracking technologies.
- [ ] The app includes a data retention policy to manage the storage and deletion of user data.
- [ ] The app includes a user consent mechanism to obtain permission for data collection and usage.
- [ ] The app includes a data breach response plan to address potential security incidents.
- [ ] The app includes a third-party services review to ensure compliance with terms and conditions.
- [ ] The app includes a regular audit schedule to review and update security, privacy, and compliance measures.
- [ ] The app includes a feedback loop to continuously gather user input and improve the app over time.
- [ ] The app includes a feature flag system to enable or disable features for testing and rollout.
- [ ] The app includes a localization review to ensure accurate translations and cultural appropriateness.
- [ ] The app includes a usability testing plan to evaluate user experience and identify areas for improvement.
- [ ] The app includes a performance monitoring plan to track and optimize app speed and responsiveness.
- [ ] The app includes a scalability plan to accommodate growth and increased user demand.
- [ ] The app includes a disaster recovery plan to ensure business continuity in the event of a major incident.
- [ ] The app includes a change management process to manage updates and modifications to the app.
- [ ] The app includes a stakeholder communication plan to keep relevant parties informed about app development and updates.
- [ ] The app includes a training plan to educate users and support staff on app features and functionality.
- [ ] The app includes a retirement plan to manage the end-of-life process for the app
- [ ] The app includes a sustainability plan to ensure long-term viability and support.
- [ ] The app includes a competitive analysis to identify strengths, weaknesses, opportunities, and threats.
- [ ] The app includes a user segmentation strategy to target specific audiences and tailor the user experience.
- [ ] The app includes a feature prioritization framework to guide development efforts and resource allocation.
- [ ] The app includes a bug tracking system to manage and resolve issues reported by users and developers.
- [ ] The app includes a version control system to manage code changes and collaboration among developers.
- [ ] The app includes a coding standards document to ensure consistency and quality in code development.
- [ ] The app includes a peer review process to ensure code quality and adherence to best practices among developers.
- [ ] The app includes a continuous integration/continuous deployment (CI/CD) pipeline to automate testing and deployment processes.
- [ ] The app includes a feature documentation process to ensure that new features are properly documented for users and developers.
- [ ] The app includes a user onboarding process to help new users get started with the app quickly and easily.
