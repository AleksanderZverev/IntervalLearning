FROM nginx:mainline
WORKDIR /app

ADD nginx.conf /etc/nginx/nginx.conf
ADD fullchain.pem /etc/nginx/fullchain.pem
ADD privkey.pem /etc/nginx/privkey.pem

EXPOSE 80
EXPOSE 443
