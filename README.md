AI EDI Error Translator

Web Application that transforms cryptic EDI (Electronic Data Interchange) technical errors into plain-English explanations using LLMs.

Flow
Input: User sends an email containing an error table to the CloudMailin address.
Webhook: CloudMailin parses the SMTP traffic and POSTs a JSON payload to the ASP.NET Core API.
Extraction: The app parses the EDI table and extracts the `Original Error` strings.
AI Processing: Errors are sent to a Gemma-2-2b-it model via Hugging Face.
Response: An HTML-formatted email is generated with an added "AI Explanation" column and sent back to the user.

Tech Stack
Backend: ASP.NET Core 8.0
AI: Hugging Face Inference API (Gemma-2-2b-it)
Email: CloudMailin (SMTP + Webhook)
DevOps: ngrok (Webhook Tunneling)
Frontend: Razor Pages + Bootstrap 5 + SweetAlert2

Version History
02/12/2025: - Integrated SweetAlert2 for frontend notifications.
  - Added Live Sample Tool: Users can now test the AI directly from the web UI using a dropdown list or custom text input.
  - Implemented full-screen loading states for AI processing.

