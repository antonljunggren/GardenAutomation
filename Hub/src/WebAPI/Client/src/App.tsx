import { createMemo, createSignal, type Component } from 'solid-js';


import logo from './logo.svg';
import styles from './App.module.css';
import Navbar from './components/Navbar';
import { Route, Router } from '@solidjs/router';
import Home from './pages/Home';
import Settings from './pages/Settings';
import { LocalizationProvider } from './LocalizationProvider';
import { NotificationProvider } from './NotificationProvider';



const App: Component = () => {

  return (
    <div class={styles.App}>
      <LocalizationProvider>
        <NotificationProvider>
          <Navbar />

          <div class='mb-16'>
            <Router>
              <Route path="/" component={Home} />
              <Route path="/settings" component={Settings} />
            </Router>
          </div>
        </NotificationProvider>
      </LocalizationProvider>
    </div>

  );
};

export default App;