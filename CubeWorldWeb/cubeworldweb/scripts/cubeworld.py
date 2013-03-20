import cgi
import os

from google.appengine.ext.webapp import template
from google.appengine.api import users
from google.appengine.ext import webapp
from google.appengine.ext.webapp.util import run_wsgi_app
from google.appengine.ext import db
from google.appengine.api import mail
from datetime import datetime, timedelta

class CubeworldRemoteServer(db.Model):
    owner = db.StringProperty()
    description = db.StringProperty()
    ip = db.StringProperty()
    port = db.IntegerProperty()
    date = db.DateTimeProperty(auto_now_add=True)

class ListServers(webapp.RequestHandler):
    def get(self):
        mindate = datetime.today() - timedelta(seconds=60)
        servers_query = CubeworldRemoteServer.all().filter('date >= ', mindate).order('-date')
        servers = servers_query.fetch(20)
        
        for server in servers:
            self.response.out.write(server.ip + ',' + str(server.port) + ',' + server.owner + ',' + server.description + ';')

class RegisterServers(webapp.RequestHandler):
    def get(self):
        mindate = datetime.today() - timedelta(seconds=60)
        db.delete(CubeworldRemoteServer.all().filter('ip = ', self.request.remote_addr).fetch(10))
        db.delete(CubeworldRemoteServer.all().filter('date < ', mindate).fetch(10))
        
        server = CubeworldRemoteServer()
        server.owner = self.request.get('owner')
        server.description = self.request.get('description')
        server.ip = self.request.remote_addr
        server.port = int(self.request.get('port'))
        
        if (len(server.owner) > 1 and len(server.description) > 1 and server.port > 0 and server.port < 65535):
            server.put()
            self.response.out.write('OK')
        else:
            self.response.out.write('ERROR')

application = webapp.WSGIApplication(
                                     [('/list', ListServers),
                                      ('/register', RegisterServers)],
                                     debug=True)

def main():
    run_wsgi_app(application)

if __name__ == "__main__":
    main()