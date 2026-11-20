# Build dell'applicazione Angular e distribuzione con nginx.
FROM node:24-alpine AS build
WORKDIR /src

# package*.json prima del resto: `npm ci` si rifa' solo quando cambiano le
# dipendenze, non a ogni modifica di un componente.
COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine AS runtime
COPY --from=build /src/dist/frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
