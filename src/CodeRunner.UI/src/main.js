import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

import './assets/main.css'

/* import the fontawesome core */
import { library } from '@fortawesome/fontawesome-svg-core'

/* import font awesome icon component */
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

/* import specific icons */
import { faPlay } from '@fortawesome/free-solid-svg-icons'
import { faTrash } from '@fortawesome/free-solid-svg-icons'
import { faCopy } from '@fortawesome/free-solid-svg-icons'
import { faAngleLeft } from '@fortawesome/free-solid-svg-icons'
import { faAngleRight } from '@fortawesome/free-solid-svg-icons'

/* add icons to the library */
library.add(faPlay, faTrash, faCopy, faAngleLeft, faAngleRight)

const app = createApp(App)

app.use(router)

app
.component('font-awesome-icon', FontAwesomeIcon)
.mount('#app')
