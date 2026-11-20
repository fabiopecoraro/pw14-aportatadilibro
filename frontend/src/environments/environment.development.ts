// Sostituisce environment.ts nelle build di sviluppo. Con `ng serve` non c'è
// nginx davanti: frontend e API stanno su due origini diverse, quindi qui gli
// indirizzi sono assoluti e puntano all'API avviata in locale. È in HTTPS, e il
// browser rifiuta le chiamate finché il certificato di sviluppo .NET non è
// attendibile sulla macchina.
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7163/api',
  filesBaseUrl: 'https://localhost:7163',
  chatHubUrl: 'https://localhost:7163/hubs/chat'
};
