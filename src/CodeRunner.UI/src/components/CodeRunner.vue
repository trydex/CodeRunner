<template>
  <div>
    <div class="editor-header">
      <div class="editor-control-container">
        <button class="editor-button run" @click="runCode">
          <font-awesome-icon icon="fa-solid fa-play" class="icon-with-margin" />  Run
        </button>
        <div class="editor-input">
          <label for="processes-input">Processes</label>
          <input dir="rtl" type="number" id="processes-input" min="1" max="10" name="processes" v-model="processCount">
        </div>
        <button class="editor-button clear" @click="clearEditor">
          <font-awesome-icon icon="fa-solid fa-trash" />
        </button>
      </div>
      <div class="editor-control-container template-switcher-container">
        <div class="script-language-dropdown">
            <select name="script-language" v-model="selectedLanguage" @change="onLanguageChange">
              <option value="csharp">C#</option>
              <option value="golang">Go</option>
            </select> 
        </div>
        <button class="editor-button template-switcher" @click="changeTemplateIndex(currentTemplateIndex - 1)">
          <font-awesome-icon icon="fa-solid fa-angle-left" />  
        </button>
        <span> {{ currentTemplateIndex + 1 }} / {{ templates.length }}</span>
        <button class="editor-button template-switcher" @click="changeTemplateIndex(currentTemplateIndex + 1)">
          <font-awesome-icon icon="fa-solid fa-angle-right" />  
        </button>
      </div>
      <div class="editor-control-container">
        <button class="editor-button copy" @click="copyToClipboard">
          <font-awesome-icon icon="fa-solid fa-copy" />
        </button>
      </div>
    </div>
    <div class="editor-container">
      <div id="input-editor" class="editor"></div>
      <div id="output-editor" class="editor">Results will be here...</div>
    </div>
  </div>
</template>

<script>

import ace from 'ace-builds/src-noconflict/ace'
import "ace-builds/src-noconflict/mode-csharp";
import "ace-builds/src-noconflict/mode-markdown";
import ExecutorApiClient from "../services/executorApiClient"
import allTemplates from "../templates"

export default {
  name: 'CodeRunner',
  mounted() {
    this.templates = allTemplates[this.selectedLanguage];

    const inputEditor = ace.edit("input-editor");
    const outputEditor = ace.edit("output-editor");

    this.inputEditor = inputEditor;
    this.outputEditor = outputEditor;

    this.configureEditor(inputEditor, "csharp", 'tomorrow_night', false);
    this.configureEditor(outputEditor, "markdown", 'chaos', true);

    this.loadTemplate(this.templates[0]);
  },
  data() {
    return {
      currentTemplateIndex: 0,
      templates: [],
      processCount: 1,
      selectedLanguage: "csharp",
      isExecuting: false,
      language2code: {
        "csharp" : 1,
        "golang": 2
      }
    }
  },
  methods: {
    changeTemplateIndex(newIndex) {
      if(newIndex >= this.templates.length) {
        newIndex = 0;
      }
      else if (newIndex < 0) {
        newIndex = this.templates.length - 1;
      }

      this.currentTemplateIndex = newIndex;
      const template = this.templates[this.currentTemplateIndex];
      this.loadTemplate(template);
    },
    loadTemplate(template) {
      this.inputEditor.setValue(template.code.trim());
      this.processCount = template.processCount;
      this.inputEditor.clearSelection();
    },
    configureEditor(editor, language, themeName, readonly) {
      editor.setTheme(`ace/theme/${themeName}`);
      editor.session.setMode(`ace/mode/${language}`);
      editor.setFontSize("14px");
      editor.session.setUseWrapMode(true);
      editor.setReadOnly(readonly); 
    },
    async runCode() {
      this.isExecuting = true;
      this.highlightLineInEditor(this.outputEditor, false);
      this.showLoader();

      const code = this.inputEditor.getValue();

      if (!this.executorApiClient)
        this.executorApiClient = new ExecutorApiClient();

      const languageTypeNumber = this.language2code[this.selectedLanguage];
      await this.executorApiClient.executeCode(code, this.processCount, languageTypeNumber, (executionResult) => {
        console.log('result received');
        console.log(executionResult);

        this.isExecuting = false;
        this.displayResult(executionResult);
        this.highlightLineInEditor(this.outputEditor, true);
      });
    },
    highlightLineInEditor(editor, enable) {
      editor.setHighlightActiveLine(enable);
      editor.setHighlightGutterLine(enable);
    },
    showLoader(dotsCount = 3) {
      if(this.isExecuting) {
        this.outputEditor.setValue('Executing' + '.'.repeat(dotsCount));
        this.outputEditor.clearSelection();

        setTimeout(this.showLoader, 100, dotsCount >= 3 ? 1 : dotsCount + 1);
      }
    },
    displayResult(executionResult) {
      const separator = "\r\n";

      this.outputEditor.setValue("");

      const doc = this.outputEditor.session.getDocument();

      if(executionResult.processResults){
        let str = `# Work report`;

        for (const processResult of executionResult.processResults) {
          str += separator + separator + `### ProcessId = ${processResult.id}` + separator;

          if(processResult.output) {
            str += separator + `Output ${separator} > ${processResult.output.trim()}`;
          }

          if(processResult.error) {
            str += `Error ${separator} > ${processResult.error.trim()}`;
          }
        }

        doc.insert({ row: doc.getLength(), column: 0 }, str);
      }

      this.outputEditor.setOption("showCursor", true);
    },
    clearEditor() {
      this.inputEditor.setValue("");
      this.outputEditor.setValue("");
    },
    async copyToClipboard() {
      await navigator.clipboard.writeText(this.outputEditor.getValue());
    },
    onLanguageChange() {
      this.currentTemplateIndex = 0;

      this.templates = allTemplates[this.selectedLanguage];

      this.loadTemplate(this.templates[this.currentTemplateIndex]);
      this.configureEditor(this.inputEditor, this.selectedLanguage, 'tomorrow_night', false);
    }
  }
}
</script>

<style>
.editor-input  {
  margin-left: 10px;
  padding: 8px 16px;
  font-size: 12px;
  font-weight: bold;
  text-transform: uppercase;
  color: #eeeaea;
  background-color: #36393d;
  border: none;
  border-radius: 4px;
}

.editor-input label {
  font-weight: bold;
  margin-right: 10px;
}

#processes-input {
  background-color: #25282c;
  color: #eeeaea;
  border: none; 
  padding: 0;
  text-align: right;
}

#processes-input:focus {
  outline: none;
}

.editor {
  width: 100%;
  min-height: 500px;
  height: 75vh;
}

.editor-container {
  display: flex;
  align-items: flex-start;
  width: 100%;
  height: 100%;
}

.editor-header {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 10px;
  background-color: #23262b;
  color: #ffffff;
}

.editor-control-container {
  display: flex;
}

.editor-button {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 8px 16px;
  font-size: 12px;
  font-weight: bold;
  text-transform: uppercase;
  color: #eeeaea;
  background-color: #36393d;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s ease-in-out;
}

.editor-button.run:hover {
  background-color: #449d44;
}

.editor-button.clear:hover {
  background-color: #bb4040;
}

.editor-button {
  margin-left: 10px;
}

.editor-button.run {
  margin-left: 0px;
}

.editor-button.copy:hover {
  background-color: #4a4e53;
}

.editor-button:active {
  filter: brightness(80%);
}

.fa {
  margin-right: 5px;
}

.icon-with-margin {
  margin-right: 10px;
}

.template-switcher-container {
  margin-left: -250px;
  display: flex;
  align-items: center;
}

.template-switcher {
  margin-left: 10px;
  margin-right: 10px;
}
</style>