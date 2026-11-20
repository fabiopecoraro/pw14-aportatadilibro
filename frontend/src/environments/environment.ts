// Configurazione della build di produzione, quella che nginx serve dal
// container. I percorsi sono relativi perché lo stesso nginx fa da proxy verso
// l'API per /api, /covers e /hubs: frontend e API condividono l'origine, quindi
// il browser non fa richieste cross-origin e l'indirizzo dell'API non resta
// fissato dentro il bundle. Per questo `filesBaseUrl` è vuoto: i percorsi delle
// copertine restituiti dal backend ("/covers/...") sono già validi così.
export const environment = {
  production: true,
  apiUrl: '/api',
  filesBaseUrl: '',
  chatHubUrl: '/hubs/chat'
};
